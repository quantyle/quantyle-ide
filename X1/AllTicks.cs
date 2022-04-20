using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace X1
{
    public class AllTicks : WebSocketBehavior
    {        public CancellationTokenSource cts;
        ConcurrentDictionary<string, Ticker> allTickers;

        Task feed;

        public AllTicks(ConcurrentDictionary<string, Ticker> allTickers)
        {
            this.allTickers = allTickers;
            this.cts = new CancellationTokenSource();
        }

        protected override void OnOpen()
        {
            Console.WriteLine("Frontend connected to aggregate feed");

            // create cancellation token to be passed to the feed task
            CancellationToken ct = this.cts.Token;

            // parallel task used to broadcast alltickers to clients
            this.feed = Task.Run(() =>
            {
                // Were we already canceled?
                while (!ct.IsCancellationRequested)
                {
                    var result = Utf8Json.JsonSerializer.Serialize(this.allTickers);
                    Sessions.Broadcast(System.Text.Encoding.UTF8.GetString(result));
                    Thread.Sleep(2000);
                }
            }, this.cts.Token); // Pass same token to Task.Run.
        }

        protected override void OnError(ErrorEventArgs e)
        {
            Console.WriteLine("AggFeed.OnError: {0}", e.Message);
            this.cts.Cancel();
        }

        protected override void OnClose(CloseEventArgs e)
        {
            Console.WriteLine("AggFeed.OnClose");
            this.cts.Cancel();

        }

        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine("AggFeed.OnMessage");
            // Console.WriteLine(e.Data);
        }

    }



}