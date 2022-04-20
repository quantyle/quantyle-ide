using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;
using NetMQ;
using NetMQ.Sockets;
using System.Threading.Tasks;


namespace X1
{
    public class UIFeed : WebSocketBehavior
    {
        Task feed;
        private Thread uiProcessThread;
        // private UIConnection connection;
        public CancellationTokenSource cts;
        private ConcurrentQueue<Ticker> tickSnapshot;
        private ConcurrentQueue<List<decimal>> candleSnapshot;
        // private ProducerConsumerQueue<byte[]> dojoResponse;
        string threadKey;
        private string productId;
        private string exchangeId;
        ProducerConsumerQueue<string> codeQueue;
        private ProducerConsumerQueue<byte[]> dojoResponse;

        string lastMessage = "";
        public UIFeed(
            string productId,
            string exchangeId,
            ConcurrentQueue<Ticker> tickSnapshot,
            ConcurrentQueue<List<decimal>> candleSnapshot,
            ProducerConsumerQueue<string> codeQueue,
            ProducerConsumerQueue<byte[]> dojoResponse
            )
        {
            this.tickSnapshot = tickSnapshot;
            this.candleSnapshot = candleSnapshot;
            this.threadKey = exchangeId + "-" + productId;
            this.exchangeId = exchangeId;
            this.productId = productId;
            this.cts = new CancellationTokenSource();
            this.codeQueue = codeQueue;
            this.dojoResponse = dojoResponse;
            // Console.WriteLine(">>> {0}", this.threadKey);
        }


        protected override void OnOpen()
        {
            Console.WriteLine("Frontend connected to: {0}", this.threadKey);

            var result = Utf8Json.JsonSerializer.Serialize(new UIPayload
            {
                count = 0,
                tickers = new List<Ticker> { },
                ohlcv = new List<decimal> { },
                snapshot = this.tickSnapshot.ToList(),
                candles = this.candleSnapshot.ToList(),
            });
            // send the initial message
            Sessions.Broadcast(System.Text.Encoding.UTF8.GetString(result));

            // create a new cts token to be passed to feed thread
            CancellationToken ct = this.cts.Token;

            // parallel task used to broadcast alltickers to clients
            this.feed = Task.Run(() =>
            {
                try
                {
                    using (var subSocket = new SubscriberSocket())
                    {
                        // subSocket.Options.ReceiveHighWatermark = 25;
                        subSocket.Connect("inproc://" + this.threadKey);
                        subSocket.Subscribe(this.threadKey);
                        while (!ct.IsCancellationRequested)
                        {
                            // first message is the threadKey / channel ID
                            byte[] message = subSocket.ReceiveFrameBytes();
                            message = subSocket.ReceiveFrameBytes();

                            // encode as UTF8 to avoid websocket 1002 error codes
                            lastMessage = System.Text.Encoding.UTF8.GetString(message);
                            Sessions.Broadcast(lastMessage);
                            // Console.WriteLine(lastMessage);
                        }
                        Console.WriteLine("Frontend disconnected from: {0}", this.threadKey);
                        // Console.WriteLine("lastMessage: {0}",lastMessage);
                    }
                }
                catch (NetMQ.EndpointNotFoundException e)
                {
                    Console.WriteLine("NetMQ Exception in {0}-{1}: {2}", exchangeId, productId, e.Message);
                }
            }, this.cts.Token); // Pass same token to Task.Run.

        }

        protected override void OnError(ErrorEventArgs e)
        {
            // threads.Remove(exchangeId + "-FEED");
            //this.connections[this.threadKey] = false;
            Console.WriteLine("UIFeed.OnError: {0}", e.Message);
            Console.WriteLine("UIFeed.OnError code: {0}", e.Exception);
            // Console.WriteLine("lastMessage: {0}",lastMessage);
            this.cts.Cancel();
        }

        protected override void OnClose(CloseEventArgs e)
        {

            Console.WriteLine("UIFeed.OnClose: {0}", this.threadKey);
            Console.WriteLine("UIFeed.OnClose code: {0}", e.Code);
            // Console.WriteLine("lastMessage: {0}", lastMessage);
            this.cts.Cancel();
            //this.uiProcessThread.Join();
            //this.cts.Cancel();
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine("UIFeed.OnMessage");

            // send to dojo thread
            this.codeQueue.Add(e.Data);

            // block/wait here for response from compiler
            byte[] response = this.dojoResponse.Take();

            try
            {
                Sessions.Broadcast(System.Text.Encoding.UTF8.GetString(response));
            }
            catch (Exception ex)
            {
                Console.WriteLine("CodeFeed.OnMessage exception: {0}", ex.Message);
            }
        }

    }



}