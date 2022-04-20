using System;
using System.Security.Permissions;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;

namespace X1
{
    public class UIFeedIB : WebSocketBehavior
    {
        private ProducerConsumerQueue<UIPayload> uiQueue;
        private ProducerConsumerQueue<string> uiQueueRX;
        private Thread uiProcessThread;
        private ConcurrentDictionary<string, Thread> threads;
        private string exchangeId;
        private string productId;
        private string threadKey;
        private bool connected;
        public long count = 0;
        public CancellationTokenSource cts;
        private ConcurrentQueue<Ticker> snapshots;
        private ConcurrentQueue<List<decimal>> candles;

        public UIFeedIB(
            string exchangeId,
            string productId,
            ProducerConsumerQueue<UIPayload> uiQueue,
            ProducerConsumerQueue<string> uiQueueRX,
            ConcurrentQueue<Ticker> snapshots,
            ConcurrentQueue<List<decimal>> candles,
            ConcurrentDictionary<string, Thread> threads)
        {
            this.uiQueue = uiQueue;
            this.uiQueueRX = uiQueueRX;
            this.threads = threads;
            this.exchangeId = exchangeId;
            this.productId = productId;
            this.snapshots = snapshots;
            this.candles = candles;
            this.threadKey = exchangeId + "-" + productId + "-FEED";
        }

        public void IBServerFeedThread(object obj)
        {
            CancellationToken token = (CancellationToken)obj;
            while (true)
            {
                // Console.WriteLine("taking from uiQueue");
                UIPayload message = uiQueue.Take();
                string payload = Utf8Json.JsonSerializer.ToJsonString(message);

                if (token.IsCancellationRequested)
                {
                    break;
                }
                else
                {
                    // Console.WriteLine(message);
                    // Console.WriteLine();
                    Send(payload);
                }

            }
        }

        protected override void OnOpen()
        {
            Console.WriteLine("IBKR: client connected to: {0} {1}", exchangeId, productId);
            Console.WriteLine(this.snapshots.Count);
            string result = JsonConvert.SerializeObject(new UIPayload
            {
                count = 0,
                tickers = new List<Ticker>{},
                ohlcv = new List<decimal>(),
                snapshot = this.snapshots.ToList(),
                candles = this.candles.ToList(),
            });
            Send(result);
            
            
            // if (snapshot.Count > 0)
            // {
            //     // the initial payload only contains the snapshot
            //     UIPayload payload = new UIPayload
            //     {
            //         count = 0,
            //         snapshot = snapshot.ToArray()
            //     };
            //     string result = JsonConvert.SerializeObject(payload);
            //     //Console.WriteLine(result);
            //     Send(result);
            // }


            // this.uiProcessThread = new Thread(
            //     () => ServerFeedThread(this.cts));
            // threads.TryAdd(this.threadKey, this.uiProcessThread);
            // this.uiProcessThread.Start();



            this.cts = new CancellationTokenSource();
            ThreadPool.QueueUserWorkItem(new WaitCallback(IBServerFeedThread), this.cts.Token);
            
            // open the flood gates
            threads.TryAdd(this.threadKey, this.uiProcessThread);
        }

        protected override void OnError(ErrorEventArgs e)
        {
            // threads.Remove(exchangeId + "-FEED");

            Console.WriteLine("OnError");
            //threads.Remove(this.threadKey, out Thread t);
            this.cts.Cancel();
        }

        protected override void OnClose(CloseEventArgs e)
        {
            Console.WriteLine("OnClose");

            //threads.Remove(this.threadKey, out Thread t);
            this.cts.Cancel();

        }

        protected override void OnMessage(MessageEventArgs e)
        {
            // send to IBKRThread.cs ui thread
            Console.WriteLine("++++++++++++++++++++++++++++++++++++ OnMessage +++++++++++++++++++++++++++");
            uiQueueRX.Add(e.Data);
            Console.WriteLine("++++++++++++++++++++++++++++++++++++ OnMessage +++++++++++++++++++++++++++");
            // 1. receive a new subscription message
            // 2. send candles and snapshots
            // 3. send subsequence messages
        }
    }



}