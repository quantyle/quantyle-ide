using System;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace X1
{
    public class CodeRequest
    {
        public string type { get; set; }
        public string source { get; set; }
    }


    public class CodeFeed : WebSocketBehavior
    {
        ProducerConsumerQueue<string> codeQueue;
        private ProducerConsumerQueue<byte[]> dojoResponse;

        public CodeFeed(
            ProducerConsumerQueue<string> codeQueue,
            ProducerConsumerQueue<byte[]> dojoResponse)
        {
            this.codeQueue = codeQueue;
            this.dojoResponse = dojoResponse;
        }


        protected override void OnOpen()
        {
            Console.WriteLine("CodeFeed.OnOpen");
        }

        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {

            Console.WriteLine("CodeFeed.Error: {0}", e.Message);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            Console.WriteLine("CodeFeed.OnClose");
            // this.cts.Cancel();

        }

        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine("CodeFeed.OnMessage");
            // Console.WriteLine(e.Data);

            // send to dojo thread
            this.codeQueue.Add(e.Data);

            // // ========================= UNCOMMENT BELLOW TO SEND CODE RESPONSE =============================
            // wait here for response
            byte[] response = this.dojoResponse.Take();

            try
            {
                Send(System.Text.Encoding.UTF8.GetString(response));
            }
            catch (Exception ex)
            {
                Console.WriteLine("CodeFeed.OnMessage exception: {0}", ex.Message);
            }

        }
    }
}