using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NetMQ;
using NetMQ.Sockets;
namespace X1.Threads
{

    class KrakenThread
    {


        // var startTime = DateTime.UtcNow;
        // var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
        // var stopWatch = new Stopwatch();
        // // Start watching CPU
        // stopWatch.Start();

        // // Meansure something else, such as .Net Core Middleware
        // await _next.Invoke(httpContext);

        // // Stop watching to meansure
        // stopWatch.Stop();
        // var endTime = DateTime.UtcNow;
        // var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

        // var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
        // var totalMsPassed = (endTime - startTime).TotalMilliseconds;
        // var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

        // var cpuUsagePercentage = cpuUsageTotal * 100;

        private static readonly int depth = 25;

        private static void printDictionary(SortedDictionary<decimal, decimal> asks, SortedDictionary<decimal, decimal> bids)
        {
            foreach (KeyValuePair<decimal, decimal> kvp in asks)
            {
                Console.WriteLine("ASK: {0}, {1}", kvp.Key, kvp.Value);
            }
            Console.WriteLine("---------");
            foreach (KeyValuePair<decimal, decimal> kvp in bids)
            {
                Console.WriteLine("BID: {0}, {1}", kvp.Key, kvp.Value);
            }
            Console.WriteLine();
        }

        private static void updateBook(
            List<List<string>> update,
            SortedDictionary<decimal, decimal> book,
            char side)
        {
            for (int i = 0; i < update.Count; i++)
            {
                decimal price = Convert.ToDecimal(update[i][0]);
                decimal volume = Convert.ToDecimal(update[i][1]);

                if (volume > 0)
                {
                    if (book.ContainsKey(price))
                    {
                        book[price] = volume; // update price level
                    }
                    else
                    {
                        book.Add(price, volume); // add price level 

                        if (book.Count > depth) // remove levels no longer in scope
                        {
                            if (side == 'b') // bid
                                book.Remove(book.First().Key); // remove bid
                            else
                                book.Remove(book.Last().Key); // remove ask
                        }
                    }
                }
                else
                {
                    book.Remove(price);  // order cancelled or executed
                }
            }
        }


        public static void FeedThread(
            int updateFreqMilliseconds,
            string productId,
            // ProductBalance productBalance,
            ConcurrentDictionary<string, Ticker> allTickers,
            ProducerConsumerQueue<List<decimal>> candlesQueue,
            ConcurrentQueue<Ticker> tickSnapshot,
            int snapshotSize,
            object cts
            )
        {
            long lastUpdateTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            long lastUpdateTimeSec = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

            try
            {
                int ohlcvResolution = 60;
                CancellationToken token = (CancellationToken)cts;
                BlockingCollection<byte[]> messageQueue = new BlockingCollection<byte[]>();
                string threadKey = "KRKN-" + productId;
                string bookUpdateCode = "book-" + depth.ToString();
                var market = new Market(updateFreqMilliseconds, threadKey);



                using (Websockets.KrakenWebsocket krakenSocket = new Websockets.KrakenWebsocket(productId, messageQueue, token, auth: false, bookDepth: depth))
                // using (Websockets.KrakenWebsocket krakenPrivateSocket = new Websockets.KrakenWebsocket(productId, messageQueue, token, auth: true))
                {
                    //  private and public connections, both sockets will add messages to messageQueue
                    krakenSocket.Connect();
                    // Console.WriteLine("KRKN Thread Started for: {0}", productId);
                    // krakenPrivateSocket.Connect();

                    // loop until we find the order book snapshots, this will save us time in the main loop
                    while (true)
                    {
                        byte[] buffer = messageQueue.Take();

                        // Console.WriteLine(buffer.Length);

                        if (buffer[0] == 91)
                        {
                            List<object> chart = Utf8Json.JsonSerializer.Deserialize<List<object>>(buffer);
                            string code = chart[2].ToString();
                            string privateCode = chart[1].ToString();

                            if (code == bookUpdateCode) // book update
                            {
                                string message = Utf8Json.JsonSerializer.ToJsonString(chart[1]);
                                KrakenBook msg = Utf8Json.JsonSerializer.Deserialize<KrakenBook>(message);

                                // check for ask updates
                                if (msg.askSnapshot != null)
                                {
                                    // Console.WriteLine("KRKN-{0}", productBalance);
                                    for (int i = 0; i < msg.askSnapshot.Count; i++)
                                    {
                                        decimal askPrice = decimal.Parse(msg.askSnapshot[i][0]);
                                        decimal askVolume = decimal.Parse(msg.askSnapshot[i][1]);
                                        market.asks.Add(askPrice, askVolume);

                                        decimal bidPrice = decimal.Parse(msg.bidSnapshot[i][0]);
                                        decimal bidVolume = decimal.Parse(msg.bidSnapshot[i][1]);
                                        market.bids.Add(bidPrice, bidVolume);
                                    }

                                    break;
                                }
                            }
                        }
                        else
                        {
                            // return the message back to the queue
                            messageQueue.Add(buffer);
                        }
                    }

                    // Console.WriteLine("============== KRKN SNAPSHOT ================");

                    // loop here forever
                    while (!token.IsCancellationRequested)
                    {
                        byte[] buffer = messageQueue.Take();


                        // the first char is either a '{' or a '[' 
                        // Conversion to UTF8: 123 = '{' , 91 = '[' 
                        if (buffer[0] == 91)
                        {
                            List<object> chart = Utf8Json.JsonSerializer.Deserialize<List<object>>(buffer);
                            string code = chart[chart.Count - 2].ToString();


                            if (code == bookUpdateCode) // book update
                            {
                                for (int i = 1; i < chart.Count - 2; i++)
                                {
                                    // [320,{"a":[["41326.30000","0.39206101","1632942805.042274"]],"c":"2842706256"},"book-25","XBT/USD"]
                                    string message = Utf8Json.JsonSerializer.ToJsonString(chart[i]);
                                    KrakenBook msg = Utf8Json.JsonSerializer.Deserialize<KrakenBook>(message);

                                    // asks
                                    if (msg.asks != null)
                                    {
                                        updateBook(msg.asks, market.asks, 'a');
                                    }
                                    // bids
                                    else
                                    {
                                        updateBook(msg.bids, market.bids, 'b');
                                    }
                                }
                            }
                            else if (code == "trade") // trade update
                            {

                                string message = Utf8Json.JsonSerializer.ToJsonString(chart[1]);
                                var array = Newtonsoft.Json.Linq.JArray.Parse(message);

                                for (int i = 0; i < array.Count; i++)
                                {
                                    // [
                                    //   "43650.80000", // price
                                    //   "0.00100000",  // volume
                                    //   "1632983198.880424", // time
                                    //   "s",  // side
                                    //   "l",  // orderType
                                    //   ""  // misc
                                    // ]
                                    List<string> recv = array[i].ToObject<List<string>>();
                                    decimal price = decimal.Parse(recv[0]);
                                    decimal lastSize = decimal.Parse(recv[1]);
                                    decimal time = decimal.Parse(recv[2]);
                                    string side = recv[3] == "s" ? "sell" : "buy";

                                    // calculate candlestick 
                                    long timestamp = Convert.ToInt64(Math.Truncate(time)); // convert "1632983198.880424" to 1632983198
                                    long ts = timestamp - (timestamp % ohlcvResolution); // t - (t mod 60) 

                                    //{"type":"ticker","sequence":28547497887,"product_id":"BTC-USD","price":"50325.69","open_24h":"49169.65","volume_24h":"8545.83395770","low_24h":"48102.89","high_24h":"50425","volume_30d":"441546.49483990","best_bid":"50325.69","best_ask":"50325.70","side":"sell","time":"2021-08-23T05:20:07.296797Z","trade_id":204550321,"last_size":"0.00230168"}

                                    // set ticker
                                    market.payload.ticker.id = time;
                                    market.payload.ticker.time = timestamp;
                                    market.payload.ticker.price = price;
                                    market.payload.ticker.size = lastSize;
                                    market.payload.ticker.side = side;

                                    // update product balance
                                    //-------------------------------------------- add product balances below -----------------------------------
                                    // payload.portfolioValue = productBalance.baseAmount * payload.ticker.price;
                                    // payload.usdAvailable = productBalance.quoteAmount;
                                    //-------------------------------------------- add product balances below -----------------------------------

                                    market.payload.portfolioValue = 0;
                                    market.payload.usdAvailable = 0;
                                    // Console.WriteLine(productBalance.quoteAmount);

                                    // add to ticker queue
                                    market.tickers.Add(
                                        new Ticker
                                        {
                                            id = time,
                                            time = timestamp,
                                            price = price,
                                            size = lastSize,
                                            side = side,
                                        }
                                    );

                                    allTickers[threadKey] = new Ticker
                                    {
                                        id = time,
                                        time = timestamp,
                                        price = price,
                                        size = lastSize,
                                        side = side,
                                    };

                                    // add to snapshot
                                    tickSnapshot.Enqueue(
                                        new Ticker
                                        {
                                            id = time,
                                            time = timestamp,
                                            price = price,
                                            size = lastSize,
                                            side = side,
                                        }
                                    );
                                    // trim the snapshot 
                                    if (tickSnapshot.Count > snapshotSize)
                                    {
                                        tickSnapshot.TryDequeue(out Ticker throwaway);
                                    }

                                    var candle = market.payload.ohlcv;
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
                                            candle[5] += lastSize;
                                        }
                                        else
                                        {
                                            candlesQueue.Add(candle);
                                            market.payload.ohlcv =
                                                new List<decimal>
                                                {
                                                Convert.ToDecimal(ts),
                                                price,
                                                price,
                                                price,
                                                price,
                                                lastSize,
                                                };
                                        }
                                    }
                                    else
                                    {
                                        market.payload.ohlcv =
                                            new List<decimal>
                                            {
                                            Convert.ToDecimal(ts),
                                            price,
                                            price,
                                            price,
                                            price,
                                            lastSize,
                                            };
                                    }
                                }
                            }
                            else if (code == "ownTrades") // private endpoint: open orders
                            {
                                string s = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                                Console.WriteLine("KRKN ownTrades: {0}", s);
                            }
                            else if (code == "openOrders") // private endpoint: open orders
                            {
                                /*
                                KRKN openOrders: [[{"OVPXPI-RC3ZU-NPNWJJ":{"avg_price":"0.00000","cost":"0.00000","descr":{"close":null,"leverage":null,"order":"buy 0.01355469 ETH/USD @ limit 2766.57000","ordertype":"limit","pair":"ETH/USD","price":"2766.57000","price2":"0.00000","type":"buy"},"expiretm":null,"fee":"0.00000","limitprice":"0.00000","misc":"","oflags":"fciq","opentm":"1642106176.943183","refid":null,"starttm":null,"status":"pending","stopprice":"0.00000","timeinforce":"GTC","userref":0,"vol":"0.01355469","vol_exec":"0.00000000"}}],"openOrders",{"sequence":2}]
                                KRKN openOrders: [[{"OVPXPI-RC3ZU-NPNWJJ":{"status":"open","userref":0}}],"openOrders",{"sequence":3}]
                                KRKN openOrders: [[{"OVPXPI-RC3ZU-NPNWJJ":{"lastupdated":"1642106190.223285","status":"canceled","vol_exec":"0.00000000","cost":"0.00000","fee":"0.00000","avg_price":"0.00000","userref":0,"cancel_reason":"User requested"}}],"openOrders",{"sequence":4}]
                                */
                                Console.WriteLine("======== KRKN openOrders ==========");
                                string message = Utf8Json.JsonSerializer.ToJsonString(chart[0]);
                                Console.WriteLine(message);

                            }
                            else
                            {
                                string message = Utf8Json.JsonSerializer.ToJsonString(chart[0]);
                                Console.WriteLine("UNKNOWN: {0}", message);
                            }

                            /*
                            OPEN ORDER:
                                [
                                    [
                                        {
                                            "OVPXPI-RC3ZU-NPNWJJ": {
                                                "avg_price": "0.00000",
                                                "cost": "0.00000",
                                                "descr": {
                                                    "close": null,
                                                    "leverage": null,
                                                    "order": "buy 0.01355469 ETH/USD @ limit 2766.57000",
                                                    "ordertype": "limit",
                                                    "pair": "ETH/USD",
                                                    "price": "2766.57000",
                                                    "price2": "0.00000",
                                                    "type": "buy"
                                                },
                                                "expiretm": null,
                                                "fee": "0.00000",
                                                "limitprice": "0.00000",
                                                "misc": "",
                                                "oflags": "fciq",
                                                "opentm": "1642106176.943183",
                                                "refid": null,
                                                "starttm": null,
                                                "status": "pending",
                                                "stopprice": "0.00000",
                                                "timeinforce": "GTC",
                                                "userref": 0,
                                                "vol": "0.01355469",
                                                "vol_exec": "0.00000000"
                                            }
                                        }
                                    ],
                                    "openOrders",
                                    {
                                        "sequence": 2
                                    }
                                ]

                            CLOSE ORDER 1/2: 
                                [
                                    [
                                        {
                                            "OVPXPI-RC3ZU-NPNWJJ": {
                                                "status": "open",
                                                "userref": 0
                                            }
                                        }
                                    ],
                                    "openOrders",
                                    {
                                        "sequence": 3
                                    }
                                ]


                            CLOSE ORDER 2/2: 
                                [
                                    [
                                        {
                                            "OVPXPI-RC3ZU-NPNWJJ": {
                                                "lastupdated": "1642106190.223285",
                                                "status": "canceled",
                                                "vol_exec": "0.00000000",
                                                "cost": "0.00000",
                                                "fee": "0.00000",
                                                "avg_price": "0.00000",
                                                "userref": 0,
                                                "cancel_reason": "User requested"
                                            }
                                        }
                                    ],
                                    "openOrders",
                                    {
                                        "sequence": 4
                                    }
                                ]

                            */


                            // else if (code == "324") // ticker update ( use trade instead)
                            // {
                            //     KrakenTrade msg = DeserializeToList<KrakenTrade>(message);
                            //     decimal price = decimal.Parse(msg.close[0].ToString());
                            //     decimal volume = decimal.Parse(msg.volume[0].ToString());
                            //     Console.WriteLine("price: {0}", price);
                            //     Console.WriteLine("volume: {0}", volume);
                            //     Console.WriteLine(message);
                            // } 

                            // SEND TO FRONTEND
                            market.updateUI();

                        }

                        // if (buffer[0] == 91) 
                        // {

                        // }

                        // }
                        // catch (Exception e)
                        // {
                        //     Console.WriteLine(e.Message);
                        // }
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("=============== KRAKEN CLOSED ============");
                Console.WriteLine(e.StackTrace);
                Console.WriteLine();
                Console.WriteLine(e.Message);
                Console.WriteLine();

            }
        }
    }
}