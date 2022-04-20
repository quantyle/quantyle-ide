using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.IO;
using System.Collections.Concurrent;

namespace X1.Websockets
{
    public class BinanceWebsocket : IDisposable
    {

        private int BufferSize = 512;
        private ClientWebSocket ws;
        private object state;
        private string productId;
        private int count;
        private string url = "wss://stream.binance.us:9443/ws";
        private BlockingCollection<byte[]> messageQueue;

        public BinanceWebsocket(string productId, BlockingCollection<byte[]> messageQueue, object state = null)
        {
            this.productId = productId;
            this.messageQueue = messageQueue;
            this.state = state;
            this.count = 0;
        }


        public Task SendSubscriptionMessage()
        {
            string formattedProductId = this.productId.ToLower().Replace("-", "");
            string json = "{\"method\": \"SUBSCRIBE\", \"params\": [\"" + formattedProductId + "@depth@100ms\", \"" + formattedProductId + "@trade\" ], \"id\": 1}";
            //string json = "{\"type\": \"subscribe\", \"product_ids\": [\"" + this.productId + "\"], \"channels\": [\"level2\", \"ticker\"]}";
            //Console.WriteLine(json);
            var encoded = Encoding.UTF8.GetBytes(json);

            // Define the cancellation token.
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            var buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);
            return ws.SendAsync(buffer, WebSocketMessageType.Text, true, token);
        }

        // public static Task SendString(ClientWebSocket ws, String data, CancellationToken cancellation)
        // {
        //     var encoded = Encoding.UTF8.GetBytes(data);
        //     var buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);
        //     return ws.SendAsync(buffer, WebSocketMessageType.Text, true, cancellation);
        // }

        // Connect to the websocket and start receiving data. Should not return while the 
        public async void Connect()
        {
            CancellationToken token = (CancellationToken)this.state;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    ws = new ClientWebSocket();
                    await ws.ConnectAsync(new Uri(url), CancellationToken.None);
                    await SendSubscriptionMessage();
                    await Receive();
                }
                catch (System.Net.WebSockets.WebSocketException)
                {
                    Console.WriteLine("BINA-{0} Websocket closed.", productId);
                    // Console.WriteLine("Coinbase Websocket closed unexpectedly, with exception: {0}", e.ToString());
                }
                ws.Dispose();
                // sleep for 1 second before trying to reconnect
                Thread.Sleep(1000);
            }
        }

        private async Task Receive()
        {

            ArraySegment<Byte> buffer = new ArraySegment<byte>(new Byte[BufferSize]);
            WebSocketReceiveResult result = null;

            while (ws.State == WebSocketState.Open)
            {
                using (var ms = new MemoryStream())
                {
                    do
                    {
                        result = await ws.ReceiveAsync(buffer, CancellationToken.None);
                        ms.Write(buffer.Array, buffer.Offset, result.Count);
                    }
                    while (!result.EndOfMessage);

                    var bytes = ms.GetBuffer();
                    if (bytes.Length > 0)
                    {
                        this.messageQueue.Add(bytes);
                    }
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// Dispose Websocket
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    //rc = null;
                    ws.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }


        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

}
