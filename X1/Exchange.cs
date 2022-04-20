using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using WebSocketSharp.Server;
using System.Linq;

namespace X1
{
    public static class Exchange
    {
        public static void run(
            string productId,
            string exchangeId,
            // ProductBalance productBalance,
            ConcurrentDictionary<string, Ticker> allTickers,
            int updateFreqMilliseconds,
            int tickSnapshotSize,
            WebSocketServer websocketServer,
            CancellationTokenSource cts)
        {

            CancellationToken token = (CancellationToken)cts.Token;

            var candlesQueue = new ProducerConsumerQueue<List<decimal>>();
            var codeQueue = new ProducerConsumerQueue<string>();
            var dojoResponse = new ProducerConsumerQueue<byte[]>();
            var tickSnapshot = new ConcurrentQueue<Ticker>();
            var candleSnapshot = new ConcurrentQueue<List<decimal>>();

            Thread feedThread = null;
            Thread chartThread = null;
            Func<string, List<List<decimal>>> chartingFunction = null;

            // Thread ticksThread = null;

            if (exchangeId == "GDAX")
            {
                // feed thread
                feedThread = new Thread(() =>
                        Threads.CoinbaseThread
                            .FeedThread(
                                updateFreqMilliseconds,
                                productId,
                                // productBalance,
                                // ticksQueue,
                                allTickers,
                                candlesQueue,
                                tickSnapshot,
                                tickSnapshotSize,
                                cts.Token));

                chartingFunction = (product) => new Clients.Coinbase().getChartList(product);
            }
            else if (exchangeId == "BINA")
            {
                // feed thread
                feedThread = new Thread(() =>
                    Threads.BinanceThread
                        .FeedThread(
                            updateFreqMilliseconds,
                            productId,
                            // productBalance,
                            allTickers,
                            candlesQueue,
                            tickSnapshot,
                            tickSnapshotSize,
                            cts.Token));

                chartingFunction = (product) => new Clients.Binance().getChartList(product);
            }
            else if (exchangeId == "KRKN")
            {
                // feed thread
                feedThread = new Thread(() =>
                    Threads.KrakenThread
                        .FeedThread(
                            updateFreqMilliseconds,
                            productId,
                            // productBalance,
                            allTickers,
                            candlesQueue,
                            tickSnapshot,
                            tickSnapshotSize,
                            cts.Token));

                chartingFunction = (product) => new Clients.Kraken().getChartList(product);
            }
            else if (exchangeId == "GMNI")
            {
                // feed thread
                feedThread = new Thread(() =>
                    Threads.GeminiThread
                        .FeedThread(
                            updateFreqMilliseconds,
                            productId,
                            // productBalance,
                            allTickers,
                            candlesQueue,
                            tickSnapshot,
                            tickSnapshotSize,
                            cts.Token));

                chartingFunction = (product) => new Clients.Gemini().getChartList(product);
            }
            else
            {
                throw new Exception("Invalid exchange ID");
            }

            // chart thread
            chartThread =
                new Thread(() =>
                        Threads.ChartThread
                            .thread(
                                exchangeId,
                                productId,
                                candlesQueue,
                                candleSnapshot,
                                chartingFunction,
                                cts.Token));


            chartThread.IsBackground = true;
            feedThread.IsBackground = true;

            // start threads
            chartThread.Start();
            feedThread.Start();

            // websocket server for frontend data feeds

            string uri = "/" + exchangeId + "/" + productId;

            // primary UI feed, that delivers hig h-speed real-time data
            websocketServer.AddWebSocketService<UIFeed>(uri,
                () =>
                    new UIFeed(
                        productId,
                        exchangeId,
                        tickSnapshot,
                        candleSnapshot,
                        codeQueue,
                        dojoResponse
                        ));

            // // code socket that waits for code from user and sends response from dojo
            // websocketServer.AddWebSocketService<CodeFeed>(uri + "/code",
            //     () =>
            //     new CodeFeed(
            //         codeQueue,
            //         dojoResponse));


            // ======== HANDLE CODE HERE =========
            while (!token.IsCancellationRequested)
            {
                // wait for code from user...
                string source = codeQueue.Take();

                // try to run code, catch compilation errors
                try
                {
                    Python python = new Python(source, candleSnapshot.ToList());
                    python.run();
                    DojoResponse result = python.getResponse();
                    dojoResponse.Add(Utf8Json.JsonSerializer.Serialize(result));

                }
                catch (CompilerException e)
                {
                    Console.WriteLine("Error compiling code: {0}", e.Message);
                    Console.WriteLine("line number: {0}", e.lineNumber);
                    DojoResponse result = new DojoResponse
                    {
                        error = e.Message,
                        errorLineNumber = e.lineNumber
                    };
                    // send the error to the frontend
                    dojoResponse.Add(Utf8Json.JsonSerializer.Serialize(result));
                }
            }
        }
    }
}

