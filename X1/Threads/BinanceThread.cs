using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NetMQ;
using NetMQ.Sockets;
// using Newtonsoft.Json;
// using System.Text.Json;
// using System.Threading.Tasks;
// using Newtonsoft.Json.Linq;
// using System.Security.Permissions;

namespace X1.Threads
{
    class BinanceThread
    {
        public static void FeedThread(
            int updateFreqMilliseconds,
            string productId,
            // ProductBalance productBalance,
            ConcurrentDictionary<string, Ticker> allTickers,
            //ProducerConsumerQueue<byte[]> messageQueue,
            ProducerConsumerQueue<List<decimal>> candlesQueue,
            ConcurrentQueue<Ticker> tickSnapshot,
            int snapshotSize,
            object cts)
        {

            var productIdFormat = productId.Replace("-", "");
            DateTimeOffset now = new DateTimeOffset(DateTime.UtcNow);
            long lastUpdateTime = now.ToUnixTimeMilliseconds();
            long lastUpdateTimeSec = now.ToUnixTimeSeconds();

            SortedDictionary<decimal, decimal> asks = new SortedDictionary<decimal, decimal>();
            SortedDictionary<decimal, decimal> bids = new SortedDictionary<decimal, decimal>();
            OHLCV ohlcv = new OHLCV
            {
                time = 0,
                open = 0,
                high = 0,
                low = 0,
                close = 0,
                volume = 0
            };
            int ohlcvResolution = 60;
            UIPayload payload = new UIPayload();
            string threadKey = "BINA-" + productId;


            //long timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            string uri = "https://api.binance.us/api/v3";
            int limit = 1000;
            string path = String.Format("{0}/depth", uri, productIdFormat);


            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("symbol", productIdFormat);
            param.Add("limit", limit.ToString());
            // check queue for messages
            byte[] buffer;
            long lastUpdateId = -1;
            List<Ticker> tickers = new List<Ticker>();
            CancellationToken token = (CancellationToken)cts;
            BlockingCollection<byte[]> messageQueue = new BlockingCollection<byte[]>();

            /*
            {"type":"ticker","sequence":28547497887,"product_id":"BTC-USD","price":"50325.69","open_24h":"49169.65","volume_24h":"8545.83395770","low_24h":"48102.89","high_24h":"50425","volume_30d":"441546.49483990","best_bid":"50325.69","best_ask":"50325.70","side":"sell","time":"2021-08-23T05:20:07.296797Z","trade_id":204550321,"last_size":"0.00230168"}
            */

            using (var pubSocket = new PublisherSocket())
            using (Websockets.BinanceWebsocket binanceSocket = new Websockets.BinanceWebsocket(productIdFormat, messageQueue, token))
            {
                // connect NetMQ socket
                pubSocket.Options.SendHighWatermark = 25;
                pubSocket.Bind("inproc://" + threadKey);
                binanceSocket.Connect();

                // loop here forever
                while (!token.IsCancellationRequested)
                {
                    //Console.WriteLine("waiting");
                    buffer = messageQueue.Take();  // BLOCK HERE UNTIL MESSAGE IS RECEIVED
                    var message = Utf8Json.JsonSerializer.Deserialize<BinanceData>(buffer);

                    // var jObject = JObject.Parse(message);
                    // var msgType = jObject.GetValue("e") == null ? "" : jObject.GetValue("e").ToString();
                    var msgType = message.e;
                    if (msgType == "depthUpdate")
                    {
                        var msg = Utf8Json.JsonSerializer.Deserialize<BinanceDepthUpdate>(buffer);
                        long sequence = msg.U;
                        if (sequence > (lastUpdateId + 1))
                        {
                            // HTTP GET and deserialize
                            var response = Requests.Get(path, parameters: param).Result;
                            string jsonContent = response.Content.ReadAsStringAsync().Result;
                            Clients.BinanceBookSnapshot snapshot = System.Text.Json.JsonSerializer.Deserialize<Clients.BinanceBookSnapshot>(jsonContent);

                            asks = new SortedDictionary<decimal, decimal>();
                            bids = new SortedDictionary<decimal, decimal>();

                            // get asks from snapshot
                            foreach (var ask in snapshot.asks)
                            {
                                decimal price = decimal.Parse(ask[0]);
                                decimal volume = decimal.Parse(ask[1]);
                                asks.Add(price, volume);

                            }
                            // get bids from snapshot
                            foreach (var bid in snapshot.bids)
                            {
                                decimal price = decimal.Parse(bid[0]);
                                decimal volume = decimal.Parse(bid[1]);
                                bids.Add(price, volume);
                            }
                            // get snapshot sequence id
                            lastUpdateId = snapshot.lastUpdateId;

                        }
                        else if ((sequence <= (lastUpdateId + 1)) && (msg.u >= (lastUpdateId + 1)))
                        {
                            //Console.WriteLine("===== here 2 =====");
                            // ASKS
                            if (msg.a != null)
                            {
                                foreach (List<string> ask in msg.a)
                                {
                                    decimal price = decimal.Parse(ask[0]);
                                    decimal volume = decimal.Parse(ask[1]);
                                    //Console.WriteLine("===== process asks =====");
                                    if (volume > 0)
                                    {
                                        if (asks.ContainsKey(price))
                                        {
                                            asks[price] = volume;
                                        }
                                        else
                                        {
                                            asks.Add(price, volume);
                                        }
                                    }
                                    else
                                    {
                                        asks.Remove(price);
                                    }
                                }
                            }
                            // BIDS
                            if (msg.b != null)
                            {
                                foreach (List<string> bid in msg.b)
                                {
                                    decimal price = decimal.Parse(bid[0]);
                                    decimal volume = decimal.Parse(bid[1]);
                                    //Console.WriteLine("===== process bids =====");
                                    if (volume > 0)
                                    {
                                        if (bids.ContainsKey(price))
                                        {
                                            bids[price] = volume;
                                        }
                                        else
                                        {
                                            bids.Add(price, volume);
                                        }
                                    }
                                    else
                                    {
                                        bids.Remove(price);
                                    }
                                }
                            }
                            lastUpdateId = msg.u;
                            //Console.WriteLine("===== here 2 ===== done");
                        }


                    }
                    else if (msgType == "trade")
                    {
                        var msg = Utf8Json.JsonSerializer.Deserialize<BinanceWSTrade>(buffer);
                        decimal price = Decimal.Parse(msg.p);
                        decimal size = Decimal.Parse(msg.q);
                        string side = msg.m ? "sell" : "buy";
                        long timestamp = (msg.T / 1000);

                        // {"e":"trade","E":1634589602485,"s":"BTCUSD","t":24317489,"p":"61263.9000","q":"0.00000500","b":664459366,"a":664458995,"T":1634589602485,"m":false,"M":true}
                        payload.ticker.id = msg.t;
                        payload.ticker.price = price;
                        payload.ticker.size = size;
                        payload.ticker.side = side; // Is the buyer the market maker?
                        payload.ticker.time = timestamp;

                        // update product balance
                        //-------------------------------------------- add product balances below -----------------------------------
                        // payload.portfolioValue = productBalance.baseAmount * payload.ticker.price;
                        // payload.usdAvailable = productBalance.quoteAmount;
                        //-------------------------------------------- add product balances below -----------------------------------
                        payload.portfolioValue = 0;
                        payload.usdAvailable = 0;

                        // add to ticker queue
                        tickers.Add(new Ticker
                        {
                            id = msg.t,
                            time = timestamp,
                            price = price,
                            size = size,
                            side = side,
                        });

                        allTickers[threadKey] = new Ticker
                        {
                            id = msg.t,
                            time = timestamp,
                            price = price,
                            size = size,
                            side = side,
                        };


                        // add to snapshot
                        tickSnapshot.Enqueue(
                            new Ticker
                            {
                                id = msg.t,
                                time = timestamp,
                                price = price,
                                size = size,
                                side = side,
                            }
                        );

                        // trim the snapshot 
                        if (tickSnapshot.Count > snapshotSize)
                        {
                            tickSnapshot.TryDequeue(out Ticker throwaway);
                        }

                        // calculate candlestick 
                        long ts = timestamp - (timestamp % ohlcvResolution); // t - (t mod 60) 
                        var candle = payload.ohlcv;

                        if (candle.Count > 0)
                        {

                            if (ts == candle[0])
                            {
                                // set candlestick "high"
                                if (price > candle[2])
                                {
                                    candle[2] = price;
                                }
                                // set candlestick "low"
                                if (price < candle[3])
                                {
                                    candle[3] = price;
                                }
                                // set candlestick "close"
                                candle[4] = price;
                                // increment candlestick "volume"
                                candle[5] += size;
                            }
                            else
                            {

                                candlesQueue.Add(candle);

                                payload.ohlcv =
                                    new List<decimal>
                                    {
                                        Convert.ToDecimal(ts),
                                        price,
                                        price,
                                        price,
                                        price,
                                        size,
                                    };

                            }
                        }
                        else
                        {
                            payload.ohlcv =
                                new List<decimal>
                                {
                                    Convert.ToDecimal(ts),
                                    price,
                                    price,
                                    price,
                                    price,
                                    size,
                                };
                        }
                    }



                    //Console.WriteLine("here");
                    long currentUpdateTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                    if (currentUpdateTime > (lastUpdateTime + updateFreqMilliseconds) && asks.Count > 15)
                    {
                        payload.book.volume = 0;
                        var c = payload.book.asks.GetLength(0) - 1;
                        for (int i = 0; i <= c; i++)
                        {
                            var a = asks.ElementAt(i);
                            payload.book.asks[c - i, 0] = a.Key; // price
                            payload.book.asks[c - i, 1] = a.Value; // volume

                            var b = bids.ElementAt(bids.Count - i - 1);
                            payload.book.bids[i, 0] = b.Key;  // price
                            payload.book.bids[i, 1] = b.Value;  // volume
                            payload.book.volume += (a.Value + b.Value);
                        }
                        payload.tickers = tickers;
                        byte[] result = Utf8Json.JsonSerializer.Serialize(payload);
                        pubSocket.SendMoreFrame(threadKey).SendFrame(result);

                        lastUpdateTime = currentUpdateTime;
                        // empty/reset tickers
                        tickers.Clear();


                    }
                }
            }

        }

    }
}