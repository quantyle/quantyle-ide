using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NetMQ;
using NetMQ.Sockets;



namespace X1.Threads
{
    class GeminiThread
    {

        public static void FeedThread(
            int updateFreqMilliseconds,
            string productId,
            // ProductBalance productBalance,
            ConcurrentDictionary<string, Ticker> allTickers,
            // ProducerConsumerQueue<byte[]> messageQueue,
            ProducerConsumerQueue<List<decimal>> candlesQueue,
            ConcurrentQueue<Ticker> tickSnapshot,
            int snapshotSize,
            object cts)
        {


            CancellationToken token = (CancellationToken)cts;
            string threadKey = "GMNI-" + productId;
            int ohlcvResolution = 60;
            byte[] buffer = new byte[] { };
            BlockingCollection<byte[]> messageQueue = new BlockingCollection<byte[]>();
            var market = new Market(updateFreqMilliseconds, threadKey);


            using (Websockets.GeminiWebsocket geminiSocket = new Websockets.GeminiWebsocket(productId, messageQueue, token))
            {
                geminiSocket.Connect();

                while (!token.IsCancellationRequested)
                {
                    buffer = messageQueue.Take();  // BLOCK HERE UNTIL MESSAGE IS RECEIVED

                    var e = Utf8Json.JsonSerializer.Deserialize<GeminiMarketData>(buffer);

                    if (e.type == "update")
                    {
                        // add or update price queue
                        foreach (var ev in e.events)
                        {
                            if (ev.type == "change")
                            {

                                //Console.WriteLine("change");
                                decimal id = ev.tid;
                                string side = ev.side;
                                decimal price = ev.price;
                                decimal size = ev.remaining;
                                if (size > 0)
                                {

                                    if (ev.side == "bid")
                                    {
                                        if (market.bids.ContainsKey(price))
                                        {
                                            market.bids[price] = size;
                                        }
                                        else
                                        {
                                            market.bids.Add(price, size);
                                        }

                                    }
                                    else
                                    {
                                        if (market.asks.ContainsKey(price))
                                        {
                                            market.asks[price] = size;
                                        }
                                        else
                                        {
                                            market.asks.Add(price, size);
                                        }
                                    }
                                }
                                else
                                {
                                    if (side == "bid") market.bids.Remove(price);
                                    else market.asks.Remove(price);
                                }
                            }
                            else if (ev.type == "trade")
                            {
                                //Console.WriteLine("trade");
                                long timestamp = e.timestamp;
                                decimal price = ev.price;
                                decimal size = ev.size;
                                string side = ev.makerSide == "bid" ? "sell" : "buy";

                                market.payload.ticker.id = ev.tid;
                                market.payload.ticker.time = timestamp;
                                market.payload.ticker.price = price;
                                market.payload.ticker.size = size;
                                market.payload.ticker.side = side;

                                // update product balance
                                //-------------------------------------------- add product balances below -----------------------------------
                                // market.payload.portfolioValue = productBalance.baseAmount * market.payload.ticker.price;
                                // market.payload.usdAvailable = productBalance.quoteAmount;
                                //-------------------------------------------- add product balances below -----------------------------------
                                market.payload.portfolioValue = 0;
                                market.payload.usdAvailable = 0;

                                // add to ticker queue
                                market.tickers.Add(
                                    new Ticker
                                    {
                                        id = ev.tid,
                                        time = timestamp,
                                        price = price,
                                        size = size,
                                        side = side,
                                    }
                                );
                                allTickers[threadKey] = new Ticker
                                {
                                    id = ev.tid,
                                    time = timestamp,
                                    price = price,
                                    size = size,
                                    side = side,
                                };

                                // add to snapshot
                                tickSnapshot.Enqueue(
                                    new Ticker
                                    {
                                        id = ev.tid,
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
                                        candle[5] += size;
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
                                                size,
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
                                            size,
                                        };

                                }
                            }
                        }
                    }
                    // update the frontend
                    market.updateUI();

                }
            }

        }
    }
}