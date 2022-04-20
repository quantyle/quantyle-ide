using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Net.Http;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Concurrent;


namespace X1.Websockets
{
    public class CoinbaseWebsocket : IDisposable
    {
        private int BufferSize = 512;
        private ClientWebSocket ws;
        private object state;
        private string productId;
        private int count;
        private string url = "wss://ws-feed.pro.coinbase.com";
        private string keyFile = "keys.json";
        private BlockingCollection<byte[]> messageQueue;
        private bool auth;

        public CoinbaseWebsocket(string productId, BlockingCollection<byte[]> messageQueue, object state = null, bool auth = false)
        {

            this.productId = productId;
            this.messageQueue = messageQueue;
            this.state = state;
            this.count = 0;
            this.auth = auth;
        }

        public Key readKeys()
        {
            StreamReader r = new StreamReader(this.keyFile);
            string json = r.ReadToEnd();
            Dictionary<string, Key> keys = JsonConvert.DeserializeObject<Dictionary<string, Key>>(json);
            r.Close();
            if (keys.ContainsKey("GDAX"))
            {
                return keys["GDAX"];
            }
            else
            {
                return new Key();
            }
        }

        public Task SendSubscriptionMessage()
        {
            long timestamp = DateTime.UtcNow.ToTimestamp();
            
            CoinbaseSubscription sub = new CoinbaseSubscription
            {
                product_ids = new List<string> { this.productId },
                channels = new List<string> { "level2", "ticker" },
                timestamp = timestamp.ToString(),
            };

            // read API keys if the file exists
            if (File.Exists(keyFile) && this.auth)
            {
                Key key = readKeys();
                // create the subscription message
                sub.channels.Add("user");
                sub.signature = Clients.CoinbaseRequests.ComputeSignature(HttpMethod.Get, key.secret, timestamp, "/users/self/verify");
                sub.key = key.key;
                sub.passphrase = key.passphrase;
            }

            string json = JsonConvert.SerializeObject(sub);
            var encoded = Encoding.UTF8.GetBytes(json);
            // Define the cancellation token.
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            var buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);
            return ws.SendAsync(buffer, WebSocketMessageType.Text, true, token);
        }


        public async void Connect()
        {
            CancellationToken token = (CancellationToken)this.state;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    ws = new ClientWebSocket();
                    await ws.ConnectAsync(new Uri(url), CancellationToken.None);
                    // Console.WriteLine("GDAX-{0} Websocket opened.", productId);
                    await SendSubscriptionMessage();
                    await Receive();

                }
                catch (System.Net.WebSockets.WebSocketException)
                {
                    Console.WriteLine("GDAX-{0} Websocket closed.", productId);
                    // Console.WriteLine("Coinbase Websocket closed unexpectedly, with exception: {0}", e.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("GDAX-{0} Websocket closed on exception: {1}", productId, e);
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
            // run while websocket is open
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
                    ws.Dispose();
                }
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Websocket() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
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
