using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
// using System.Linq;
// using NetMQ;
// using NetMQ.Sockets;

namespace X1.Threads
{
    class CoinbaseThread
    {

        // @TODO surround coinbasethread in try catch 
        // to avoid errors in CoinbaseMarketData e = Utf8Json.JsonSerializer.Deserialize<CoinbaseMarketData>(buffer);

        public static void FeedThread(
            int updateFreqMilliseconds,
            string productId,
            ConcurrentDictionary<string, Ticker> allTickers,
            // ProducerConsumerQueue<Ticker> ticksQueue,
            ProducerConsumerQueue<List<decimal>> candlesQueue,  // remove this once ticksQueue is working, only calculate and send OHLC through UIFeed
            ConcurrentQueue<Ticker> tickSnapshot,
            int snapshotSize,
            object cts)
        {

            CancellationToken token = (CancellationToken)cts;
            string threadKey = "GDAX-" + productId;
            int ohlcvResolution = 60;
            byte[] buffer = new byte[] { };
            BlockingCollection<byte[]> messageQueue = new BlockingCollection<byte[]>();
            var market = new Market(updateFreqMilliseconds, threadKey);
            // orders
            Dictionary<string, Order> openOrders = new Dictionary<string, Order>();
            List<Order> fillOrders = new List<Order>();

            // ===========================================================================================================
            // @TODO: openOrders should be loaded before hand, and passed here
            // maybe openOrders should be a concurrentQueue???
            // ===========================================================================================================


            try
            {
                // loop here forever
                using (Websockets.CoinbaseWebsocket coinbaseSocket = new Websockets.CoinbaseWebsocket(productId, messageQueue, token, auth: false))
                {

                    coinbaseSocket.Connect();
                    while (!token.IsCancellationRequested)
                    {
                        // block here until message is recieved
                        buffer = messageQueue.Take(); 

                        //Console.WriteLine(message);
                        // var e = message.Json<CoinbaseMarketData>();  // @TODO: reduce the latency caused by this line

                        CoinbaseMarketData e = Utf8Json.JsonSerializer.Deserialize<CoinbaseMarketData>(buffer);

                        // MEASURE LATENCY OF READ
                        // watch.Stop();
                        // double ticks = watch.ElapsedTicks;
                        // double microseconds = (ticks / System.Diagnostics.Stopwatch.Frequency) * 1000000;
                        // double nanoseconds = (ticks / System.Diagnostics.Stopwatch.Frequency) * 1000000000;

                        // // Console.WriteLine($"Execution Time: {nanoseconds} ns");
                        // Console.WriteLine($"Execution Time: {microseconds} us");


                        if (e.Type == "l2update") // book update
                        {
                            // Console.WriteLine(">>>");
                            for (int i = 0; i < e.Changes.Count; i++)
                            {
                                List<string> change = e.Changes[i];
                                string side = change[0];
                                decimal price = decimal.Parse(change[1]);
                                decimal volume = decimal.Parse(change[2]);

                                if (side == "buy")
                                {
                                    if (volume > 0)
                                    {
                                        if (market.bids.ContainsKey(price))
                                        {
                                            market.bids[price] = volume;
                                        }
                                        else
                                        {
                                            market.bids.Add(price, volume);
                                        }
                                    }
                                    else
                                    {
                                        market.bids.Remove(price);
                                    }
                                }
                                else
                                {
                                    if (volume > 0)
                                    {
                                        if (market.asks.ContainsKey(price))
                                        {
                                            market.asks[price] = volume;
                                        }
                                        else
                                        {
                                            market.asks.Add(price, volume);
                                        }
                                    }
                                    else
                                    {
                                        market.asks.Remove(price);
                                    }
                                }
                            }
                        }
                        else if (e.Type == "ticker")  // trade update
                        {
                            // Console.WriteLine(e.price);
                            // Console.WriteLine(e.lastSize);
                            // Console.WriteLine(message);
                            // Console.WriteLine();
                            decimal t1 = decimal.Parse(DateTime.Parse(e.time, null, System.Globalization.DateTimeStyles.RoundtripKind).ToTimestampMs().ToString()) / 1000;

                            market.payload.ticker.id = e.id;
                            market.payload.ticker.time = t1;
                            market.payload.ticker.price = e.price;
                            market.payload.ticker.size = e.lastSize;
                            market.payload.ticker.side = e.side;

                            // update product balance
                            //-------------------------------------------- add product balances below -----------------------------------
                            // market.payload.portfolioValue = productBalance.baseAmount * market.payload.ticker.price;
                            // market.payload.usdAvailable = productBalance.quoteAmount;
                            //-------------------------------------------- add product balances below -----------------------------------
                            market.payload.portfolioValue = 0;
                            market.payload.usdAvailable = 0;

                            // Console.WriteLine(t1);
                            // add to snapshot
                            tickSnapshot.Enqueue(
                                new Ticker
                                {
                                    id = e.id,
                                    time = t1,
                                    price = e.price,
                                    size = e.lastSize,
                                    side = e.side,
                                }
                            );
                            // trim the snapshot 
                            if (tickSnapshot.Count > snapshotSize)
                            {
                                tickSnapshot.TryDequeue(out Ticker throwaway);
                            }

                            // ticksQueue.Add(
                            //     new Ticker
                            //     {
                            //         id = e.id,
                            //         time = t1,
                            //         price = e.price,
                            //         size = e.lastSize,
                            //         side = e.side,
                            //     }
                            // );


                            // add to ticker queue
                            market.tickers.Add(
                                new Ticker
                                {
                                    id = e.id,
                                    time = t1,
                                    price = e.price,
                                    size = e.lastSize,
                                    side = e.side,
                                }
                            );

                            allTickers[threadKey] = new Ticker
                            {
                                id = e.id,
                                time = t1,
                                price = e.price,
                                size = e.lastSize,
                                side = e.side,
                            };

                            // calculate candlestick 
                            decimal timestamp = DateTime.Parse(e.time, null, System.Globalization.DateTimeStyles.RoundtripKind).ToTimestamp();
                            decimal ts = timestamp - (timestamp % ohlcvResolution); // t - (t mod 60) 
                            var candle = market.payload.ohlcv;

                            if (candle.Count > 0)
                            {
                                if (ts == candle[0])
                                {
                                    // set candlestick "high"
                                    if (e.price > candle[2])
                                    {
                                        candle[2] = e.price;
                                    }
                                    // set candlestick "low"
                                    if (e.price < candle[3])
                                    {
                                        candle[3] = e.price;
                                    }
                                    // set candlestick "close"
                                    candle[4] = e.price;
                                    // increment candlestick "volume"
                                    candle[5] += e.lastSize;
                                }
                                else
                                {
                                    candlesQueue.Add(candle);
                                    market.payload.ohlcv =
                                        new List<decimal>
                                        {
                                        ts,
                                        e.price, // open
                                        e.price, // high
                                        e.price, // low
                                        e.price,  // close
                                        e.lastSize, // volume
                                        };
                                }
                            }
                            else
                            {
                                market.payload.ohlcv =
                                    new List<decimal>
                                    {
                                    ts,
                                    e.price,
                                    e.price,
                                    e.price,
                                    e.price,
                                    e.lastSize,
                                    };
                            }
                        }
                        else if (e.UserId != null) // user messages
                        {
                            var msg = System.Text.Encoding.Default.GetString(buffer);
                            Console.WriteLine("user message: {0}", msg);
                            CoinbaseOrder order = Utf8Json.JsonSerializer.Deserialize<CoinbaseOrder>(buffer);

                            if (order.type == "received")
                            {
                                // Console.WriteLine("RECEIVED:\n price: {0}, size: {1} ", order.price, order.size);
                            }
                            else if (order.type == "open")
                            {
                                // Console.WriteLine("OPEN:\n price: {0}, size: {1} ", order.price, order.size);
                                // openOrders.Add(order);

                                openOrders.Add(order.orderId, new Order
                                {
                                    id = order.orderId,
                                    time = order.time,
                                    type = order.orderType,
                                    side = order.side,
                                    price = order.price,
                                    size = order.size, // <------------ remaining size ???
                                    status = "open",
                                });
                            }
                            else if (order.type == "done")
                            {
                                // Console.WriteLine("DONE {0}:\n  price: {1}, size: {2} ", order.reason, order.price, order.remainingSize);
                                Console.WriteLine(order.reason);
                                Console.WriteLine(order.price);
                                Console.WriteLine(order.remainingSize);

                                if (order.reason == "filled")
                                {
                                    Console.WriteLine("order filled");
                                }
                                else if (order.reason == "canceled")
                                {
                                    // remove from open orders
                                    Console.WriteLine("order cancelled");
                                    openOrders.Remove(order.orderId);
                                    Console.WriteLine("removed from open orders");
                                }
                            }
                            else if (order.type == "match")
                            {
                                Console.WriteLine("DONE {0}:\n price: {1}, size: {2}", order.price, order.remainingSize);
                            }



                            // // PLACE ORDER:
                            // user message: {
                            //     "order_id":"94a25842-3500-4d83-a926-c448056b60fd",
                            //     "order_type":"limit",
                            //     "size":"0.19815055",
                            //     "price":"2510.78",
                            //     "client_oid":"33670c27-01ce-403d-f541-91937b552782",
                            //     "type":"received",
                            //     "side":"buy",
                            //     "product_id":"ETH-USD",
                            //     "time":"2022-02-23T21:19:03.741472Z",
                            //     "sequence":26093704900,
                            //     "profile_id":"38ce5895-674c-4982-a89b-4b73f3f29a21",
                            //     "user_id":"5d675498c6274303893a805d"
                            // }

                            // user message: {
                            //     "price":"2510.78",
                            //     "order_id":"94a25842-3500-4d83-a926-c448056b60fd",
                            //     "remaining_size":"0.19815055",
                            //     "type":"open",
                            //     "side":"buy",
                            //     "product_id":"ETH-USD",
                            //     "time":"2022-02-23T21:19:03.741472Z",
                            //     "sequence":26093704901,
                            //     "profile_id":"38ce5895-674c-4982-a89b-4b73f3f29a21",
                            //     "user_id":"5d675498c6274303893a805d"
                            // }

                            // // CANCEL ORDER:
                            // user message: {
                            //     "order_id":"94a25842-3500-4d83-a926-c448056b60fd",
                            //     "reason":"canceled",
                            //     "price":"2510.78",
                            //     "remaining_size":"0.19815055",
                            //     "type":"done",
                            //     "side":"buy",
                            //     "product_id":"ETH-USD",
                            //     "time":"2022-02-23T21:19:07.659145Z",
                            //     "sequence":26093706727,
                            //     "profile_id":"38ce5895-674c-4982-a89b-4b73f3f29a21",
                            //     "user_id":"5d675498c6274303893a805d"
                            //     }

                        }
                        else if (e.Type == "snapshot")
                        {
                            // reset the ask and bids sorted dictionaries
                            market.asks = new SortedDictionary<decimal, decimal>();
                            market.bids = new SortedDictionary<decimal, decimal>();

                            // set the initial order book using the snapshot data
                            foreach (var bid in e.Bids)
                            {
                                market.bids.Add(decimal.Parse(bid[0]), decimal.Parse(bid[1]));
                            }
                            foreach (var ask in e.Asks)
                            {
                                market.asks.Add(decimal.Parse(ask[0]), decimal.Parse(ask[1]));
                            }
                        }

                        // update the frontend
                        market.updateUI();
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("=============== ERROR GDAX-{0} ===========", productId);
                Console.WriteLine(e.Message);
                Console.WriteLine();
                Console.WriteLine(e.StackTrace);
                Console.WriteLine();
                Console.WriteLine(e);
                Console.WriteLine("LAST MESSAGE {0}: {1}", productId, System.Text.Encoding.Default.GetString(buffer));

                // Thread.sleep();
            }

        }
    }
}