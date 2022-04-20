using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Net;

namespace X1.Threads
{

    public class SubscriptionMessage
    {
        public string exchange_id { get; set; }
        public string product_id { get; set; }
    }

    public class BidAskTick
    {
        public long time { get; set; }
        public decimal bidPrice { get; set; }
        public decimal askPrice { get; set; }
        public decimal bidSize { get; set; }
        public decimal askSize { get; set; }
    }

    class IBKRThread
    {
        private const int ohlcvResolution = 60;


        public int ReadInt(Stream stream)
        {
            return IPAddress.NetworkToHostOrder(new BinaryReader(stream).ReadInt32());
        }

        public void checkForMarketClose()
        {
            var date = DateTime.Now;
            if (date.Hour >= 20 || date.Hour < 8)
            {
                var DateTime8 = date.Date.AddHours((date.Hour > 8) ? 24 + 8 : 8);
                TimeSpan diff = DateTime8 - date;
            }
        }

        // BTC-USD matches example:
        // 1633159892.105762,sell,0.000115,47779
        // 1633159893.625131,buy,0.00069166,47778.74
        // 1633159894.082802,buy,0.00028131,47778.74
        // 1633159894.091017,buy,0.00010424,47778.74
        // 1633159894.772851,buy,0.0001,47778.74
        // 1633159894.787884,sell,0.00021775,47778.73
        // 1633159894.795584,sell,0.000156,47778.73
        // 1633159894.795584,sell,0.000014,47773.96
        // 1633159896.748242,buy,0.00320428,47775.45
        // 1633159898.362521,sell,0.000105,47775.44
        // 1633159899.067491,buy,0.00108315,47775.45
        // 1633159899.712879,sell,0.00019393,47771.8
        // 1633159900.327975,buy,0.04176129,47771.81
        // 1633159900.578309,buy,0.0001,47773.85

        // BTC-USD book example (change format in the future):
        // {"asks": [[47784.64, 0.00944171], [47784.61, 0.09487036], [47784.6, 0.09742423], [47784.08, 0.258], [47783.85, 0.00151549], [47783.69, 0.12877002], [47783.31, 0.00385299], 
        // [47782.17, 0.0084883], [47781.97, 0.31411807], [47781.95, 0.20931], [47780.99, 0.05233667], [47780.03, 0.01705513], [47780.0, 0.16], [47779.98, 0.208], [47778.74, 0.05900165]],
        // "bids": [[47778.73, 0.139756], [47773.96, 0.03054021], [47773.16, 0.01696497], [47773.15, 0.16], [47771.8, 0.0841], [47770.59, 0.209308], [47769.02, 0.209313], [47768.73, 0.12666674],
        // [47768.3, 0.09805203], [47768.29, 0.09696326], [47768.28, 0.77772259], [47767.65, 0.31411807], [47766.49, 0.00162], [47766.19, 0.01101343], [47766.14, 1.0255]],
        // "volume": 4.92382292, "time": 1633159893.395533}

        public void updateChart(UIPayloadIB payload, ConcurrentQueue<Candle> candles)
        {

            if (decimal.TryParse(payload.ticker.time.ToString(), out decimal timestamp))
            {
                // calculate candlestick 
                decimal ts = timestamp - (timestamp % ohlcvResolution); // t - (t mod 60) 
                var candle = payload.ohlcv;
                if (candle.Count > 0)
                {

                    if (ts == candle[0])
                    {
                        // set candlestick "high"
                        if (payload.ticker.price > candle[2])
                        {
                            candle[2] = payload.ticker.price;
                        }
                        // set candlestick "low"
                        if (payload.ticker.price < candle[3])
                        {
                            candle[3] = payload.ticker.price;
                        }
                        // set candlestick "close"
                        candle[4] = payload.ticker.price;
                        // increment candlestick "volume"
                        candle[5] += payload.ticker.size;
                    }
                    else
                    {
                        payload.ohlcv =
                            new List<decimal>
                            {
                                ts,
                                payload.ticker.price,
                                payload.ticker.price,
                                payload.ticker.price,
                                payload.ticker.price,
                                payload.ticker.size,
                            };
                    }
                }
                else
                {
                    payload.ohlcv =
                        new List<decimal>
                        {
                            ts,
                            payload.ticker.price,
                            payload.ticker.price,
                            payload.ticker.price,
                            payload.ticker.price,
                            payload.ticker.size,
                        };
                }
            }
        }

        public void updateBook(UIPayloadIB payload, List<string> msg)
        {
            int position = Convert.ToInt32(msg[3]);
            int operation = Convert.ToInt32(msg[4]);
            int side = Convert.ToInt32(msg[5]);
            decimal price = decimal.Parse(msg[6]);
            decimal size = decimal.Parse(msg[7]);

            // operation:
            // 0 = insert (insert this new order into the row identified by 'position')·
            // 1 = update (update the existing order in the row identified by 'position')·
            // 2 = delete (delete the existing order at the row identified by 'position').
            if (side == 0) // ask
            {
                if (operation == 0)
                {
                    payload.book.asks.Insert((position), new List<decimal> {
                                            price, // price  
                                            size // size
                                        });
                    payload.book.volume += size;
                }
                else if (operation == 1)  // 1 = update 
                {
                    // swap out old volume with new volume from total volume
                    payload.book.volume -= payload.book.asks[position][1];
                    payload.book.volume += size;

                    payload.book.asks[position][0] = price;
                    payload.book.asks[position][1] = size;
                }
                else if (operation == 2) // 2 = delete
                {
                    payload.book.volume -= payload.book.asks[position][1];
                    payload.book.asks.RemoveAt(position);
                }
            }
            else
            { // bid
                if (operation == 0)
                {
                    payload.book.bids.Insert(position, new List<decimal> {
                                            price, // price  
                                            size // size
                                        });
                    payload.book.volume += size;
                }
                else if (operation == 1)  // 1 = update 
                {
                    // swap out old volume with new volume from total volume
                    payload.book.volume -= payload.book.bids[position][1];
                    payload.book.volume += size;

                    payload.book.bids[position][0] = price;
                    payload.book.bids[position][1] = size;
                }
                else if (operation == 2) // 2 = delete
                {
                    payload.book.volume -= payload.book.bids[position][1];
                    payload.book.bids.RemoveAt(position);
                }
            }
        }

        public List<string> ReadStream(Stream stream)
        {
            int msgSize = ReadInt(stream);
            byte[] data = new BinaryReader(stream).ReadBytes(msgSize);
            List<string> msg = new List<string>();
            string decoded = "";
            for (var i = 0; i < msgSize; i++)
            {
                // we reached the end of the string (EOL)
                if (data[i] == 0)
                {
                    msg.Add(decoded);
                    decoded = "";
                }
                else
                {
                    decoded += System.Text.Encoding.UTF8.GetString(new byte[] { data[i] });
                }
            }
            return msg;
        }


        public void FeedThread(
            int updateFreqMilliseconds,
            string productId,
            string ibThreadKey,
            ProducerConsumerQueue<string> ibMessageQueue,
            ProducerConsumerQueue<byte[]> uiQueue,
            ProducerConsumerQueue<string> uiQueueRX,
            ConcurrentQueue<Ticker> snapshots,
            ConcurrentQueue<Candle> candles,
            ConcurrentDictionary<string, bool> connections)
        {

            try
            {
                ProducerConsumerQueue<string> historicalDataQueue = new ProducerConsumerQueue<string>();
                ConcurrentDictionary<string, SubscriptionMessage> userSubscriptionQueue = new ConcurrentDictionary<string, SubscriptionMessage>();
                IBKR.IBWebsocket ib = new IBKR.IBWebsocket();
                List<Ticker> tickers = new List<Ticker>();

                // this thread waits for send() events from the frontends' websocket connection
                Thread uiThread =
                    new Thread(() =>
                    {
                        while (true)
                        {
                            string message = uiQueueRX.Take();
                            SubscriptionMessage subscription = System.Text.Json.JsonSerializer.Deserialize<SubscriptionMessage>(message);
                            userSubscriptionQueue.TryAdd("primary-user", subscription);
                            Console.WriteLine("========== new subscription message =============");
                            Console.WriteLine(message);
                            Console.WriteLine(" exchange_id: {0}", subscription.exchange_id);
                            Console.WriteLine(" product_id: {0}", subscription.product_id);
                            Console.WriteLine("");
                            //Thread.Sleep(2000);
                        }
                    });


                string[] keyWords = productId.Split('-');
                // Console.WriteLine(keyWords[0]);
                // Console.WriteLine(keyWords[1]);
                // Console.WriteLine(keyWords[2]);
                // Console.WriteLine(keyWords[3]);
                // Console.WriteLine(keyWords[4]);

                // ib.ReqHistoricalTicks("MES", "FUT", "GLOBEX", whatToShow: "TRADES", lastTradeDateOrContractMonth: "202112");
                // ib.ReqTickByTick(symbol: keyWords[2], secType: keyWords[0], exchange: keyWords[1], lastTradeDateOrContractMonth: keyWords[4]);
                // ib.ReqHistoricalData(keyWords[1], keyWords[0], "GLOBEX", lastTradeDateOrContractMonth: "202112");
                // ib.ReqMarketDepth(keyWords[1], keyWords[0], "GLOBEX", lastTradeDateOrContractMonth: "202112");
                // ib.ReqTickByTick(keyWords[1], keyWords[0], "GLOBEX", lastTradeDateOrContractMonth: "202112");


                uiThread.Start();

                ib.Connect();
                ib.StartApi();
                Thread.Sleep(100);

                // subscribe to necessary data feeds from IB API
                // 1. historical data for candles
                // 2. market depth for order book
                // 3. account summary for net portfolilo values
                // 4. market data to tickers

                ib.ReqHistoricalData(symbol: keyWords[2], secType: keyWords[0], exchange: keyWords[1], lastTradeDateOrContractMonth: keyWords[4]);
                ib.ReqMarketDepth(symbol: keyWords[2], secType: keyWords[0], exchange: keyWords[1], lastTradeDateOrContractMonth: keyWords[4]);
                ib.ReqAccountSummary();
                ib.ReqMktData(symbol: keyWords[2], secType: keyWords[0], exchange: keyWords[1], lastTradeDateOrContractMonth: keyWords[4]);




                long lastUpdateTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                long lastUpdateTimeSec = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

                UIPayloadIB payload = new UIPayloadIB();


                // loop here forever (this is a blocking loop so the CPU don't explode)
                while (true)
                {
                    List<string> msg = ReadStream(ib.stream);
                    int msgType = Int32.Parse(msg[0]);

                    if (msgType == IBKR.IBIncomingMessage.Tickstring)
                    {
                        int tickType = Int32.Parse(msg[3]);
                        if (tickType == IBKR.TickType.LAST_TIMESTAMP)
                        {
                            int lastTimestamp = Int32.Parse(msg[4]);
                            payload.ticker.time = decimal.Parse(msg[4]);
                        }
                    }
                    else if (msgType == IBKR.IBIncomingMessage.TickSize) // TickSize
                    {
                        // Console.WriteLine("============== reqMktData (TickSize) ==============");
                        // int tickType = Int32.Parse(msg[3]);
                        // payload.ticker.size = Decimal.Parse(msg[4]);
                    }
                    else if (msgType == IBKR.IBIncomingMessage.TickPrice) // TickPrice
                    {
                        //Console.WriteLine("============== reqMktData (TickPrice) ==============");
                        int tickType = Int32.Parse(msg[3]);
                        decimal price = Decimal.Parse(msg[4]);
                        decimal size = Decimal.Parse(msg[5]);

                        if (size > 0)
                        {
                            if (tickType == IBKR.TickType.BID)
                            {

                                payload.ticker.side = "sell";
                                payload.ticker.price = price;
                                payload.ticker.size = size;


                                // add to ticker queue
                                tickers.Add(payload.ticker);
                                // add to snapshot
                                snapshots.Enqueue(payload.ticker);
                                // trim the snapshot 
                                if (snapshots.Count > 75)
                                {
                                    snapshots.TryDequeue(out Ticker throwaway);
                                }

                                // update charts
                                updateChart(payload, candles);

                            }
                            else if (tickType == IBKR.TickType.ASK)
                            {
                                payload.ticker.side = "buy";
                                payload.ticker.price = price;
                                payload.ticker.size = size;

                                // add to ticker queue
                                tickers.Add(payload.ticker);
                                if (tickers.Count > 100)
                                {
                                    tickers.RemoveAt(0);
                                }
                                // add to snapshot
                                snapshots.Enqueue(payload.ticker);
                                // trim the snapshot 
                                if (snapshots.Count > 75)
                                {
                                    snapshots.TryDequeue(out Ticker throwaway);
                                }

                                // update charts
                                updateChart(payload, candles);
                            }
                        }
                    }
                    else if (msgType == IBKR.IBIncomingMessage.TickByTick)
                    {
                        Console.WriteLine("============== reqTickByTick ==============");
                        // payload.ticker.time = msg[3];
                        // payload.ticker.price = Decimal.Parse(msg[4]);
                        // payload.ticker.size = 0;

                    }
                    else if (msgType == IBKR.IBIncomingMessage.MarketDepth)  // Order Book update
                    {
                        // Console.WriteLine("============== reqMarketDepth ==============");
                        // Console.WriteLine(price);
                        // Console.WriteLine(size);
                        // Console. WriteLine(operation);

                        updateBook(payload, msg);
                    }
                    else if (msgType == IBKR.IBIncomingMessage.AccountSummary)
                    {
                        Console.WriteLine("============== reqAccountSummary ==============");
                        string accountAttribute = msg[4];
                        if (accountAttribute == "NetLiquidation")
                        {
                            Console.WriteLine("NetLiquidation {0}", msg[5]);
                            payload.portfolioValue = Decimal.Parse(msg[5]);
                        }
                        else if (accountAttribute == "BuyingPower")
                        {
                            Console.WriteLine("BuyingPower {0}", msg[5]);
                        }
                        else if (accountAttribute == "TotalCashValue")
                        {
                            Console.WriteLine("TotalCashValue {0}", msg[5]);
                        }
                        else if (accountAttribute == "AccountType")
                        {
                            Console.WriteLine("AccountType {0}", msg[5]);
                        }
                        else if (accountAttribute == "AccruedCash")
                        {
                            Console.WriteLine("AccruedCash {0}", msg[5]);
                        }
                        // foreach (var m in msg)
                        // {
                        //     Console.WriteLine(m);
                        // }

                    }
                    else if (msgType == IBKR.IBIncomingMessage.AccountSummaryEnd)
                    {
                        Console.WriteLine("============== reqAccountSummary (End) ==============");
                        foreach (var m in msg)
                        {
                            Console.WriteLine(m);
                        }
                    }
                    else if (msgType == IBKR.IBIncomingMessage.Error) // error mesage
                    {
                        Console.WriteLine("============== ERROR ==============");
                        //int errorCode = Int32.Parse(msg[3]);
                        string errorMsg = msg[4];
                        Console.WriteLine("ERROR {0}", errorMsg);
                    }
                    else if (msgType == IBKR.IBIncomingMessage.HistoricalData) // reqHistoricalData update
                    {
                        Console.WriteLine("============== reqHistoricalData ==============");
                        msg.RemoveRange(0, 5); // ignore the first few messages
                                               //List<Candle> candles = new List<Candle>();
                                               //List<IBKR.IBCandle> candles = new List<IBKR.IBCandle>();
                        for (int i = 0; i < msg.Count; i += 8)
                        {
                            Candle candle = new Candle
                            {
                                time = long.Parse(msg[i + 0]),
                                open = decimal.Parse(msg[i + 1]),
                                high = decimal.Parse(msg[i + 2]),
                                low = decimal.Parse(msg[i + 3]),
                                close = decimal.Parse(msg[i + 4]),
                                volume = decimal.Parse(msg[i + 5]),
                            };

                            candles.Enqueue(candle);
                        }

                    }
                    else if (msgType == IBKR.IBIncomingMessage.HistoricalTick)  // reqHistoricalTicks update 
                    {
                        Console.WriteLine("============== reqHistoricalTick ==============");
                        msg.RemoveRange(0, 3);
                        msg.RemoveAt(msg.Count - 1);
                        long date = long.Parse(msg[0]);
                        //Console.WriteLine(date);
                        for (int i = 0; i < msg.Count; i += 4)
                        {
                            Console.WriteLine("time: {0}", msg[i]);
                            Console.WriteLine("price: {0}", msg[i + 2]);
                            Console.WriteLine("size: {0}", msg[i + 3]);
                        }
                        Console.WriteLine("");
                    }
                    else if (msgType == IBKR.IBIncomingMessage.HistoricalTickBidAsk)  // reqHistoricalTicks update 
                    {
                        Console.WriteLine("============== reqHistoricalTickBidAsk ==============");
                        msg.RemoveRange(0, 3);
                        msg.RemoveAt(msg.Count - 1);
                        long date = long.Parse(msg[0]);
                        //Console.WriteLine(date);
                        // foreach (var m in msg)
                        // {
                        //     Console.WriteLine(m);
                        // }
                        List<BidAskTick> bidAskTickList = new List<BidAskTick>();

                        for (int i = 0; i < msg.Count; i += 6)
                        {
                            bidAskTickList.Add(new BidAskTick
                            {
                                time = long.Parse(msg[i]),
                                bidPrice = Decimal.Parse(msg[i + 2]),
                                askPrice = Decimal.Parse(msg[i + 3]),
                                bidSize = Decimal.Parse(msg[i + 4]),
                                askSize = Decimal.Parse(msg[i + 5]),
                            });
                            Console.WriteLine("time: {0}", msg[i]);
                            Console.WriteLine("bid price: {0}", msg[i + 2]);
                            Console.WriteLine("ask price: {0}", msg[i + 3]);
                            Console.WriteLine("bid size: {0}", msg[i + 4]);
                            Console.WriteLine("ask size: {0}", msg[i + 5]);
                        }
                        // Console.WriteLine("");

                    }
                    else if (msgType == IBKR.IBIncomingMessage.HistoricalTickLast)  // reqHistoricalTicks update 
                    {
                        Console.WriteLine("============== reqHistoricalTickLast ==============");
                        msg.RemoveRange(0, 3);
                        long date = long.Parse(msg[0]);
                        //Console.WriteLine(date);
                        foreach (var m in msg)
                        {
                            Console.WriteLine(m);
                        }
                        // for (int i = 0; i < msg.Count; i += 4){
                        //     Console.WriteLine("historical tick1: {0}", msg[i]);
                        //     Console.WriteLine("historical tick2: {0}", msg[i+1]);
                        //     Console.WriteLine("historical tick3: {0}", msg[i+2]);
                        //     Console.WriteLine("historical tick3: {0}", msg[i+3]);
                        // }
                        Console.WriteLine("");
                    }
                    else
                    {
                        Console.WriteLine("============== NOT FOUND ==============");
                        foreach (var m in msg)
                        {
                            Console.WriteLine(m);
                        }
                    }



                    // send to the frontend
                    if (connections[ibThreadKey])
                    {
                        long currentUpdateTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                        if (currentUpdateTime > (lastUpdateTime + updateFreqMilliseconds))
                        {
                            //string result = JsonConvert.SerializeObject(payload);
                            payload.tickers = tickers;
                            uiQueue.Add(Utf8Json.JsonSerializer.Serialize(payload));
                            //t.Interrupt();
                            // Console.WriteLine(result);
                            // Console.WriteLine("------------------");
                            lastUpdateTime = currentUpdateTime;
                            // empty/reset tickers
                            tickers.Clear();
                        }
                    }
                }


            }
            catch (System.Net.Sockets.SocketException)
            {
                Console.WriteLine("=========== ERROR: IBGateway / TWS is not running. =========");
                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}