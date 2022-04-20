using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.IO;
using System.Collections.Concurrent;

namespace X1.Websockets
{

    public class KrakenWebsocket : IDisposable
    {

        private int BufferSize = 512;
        private ClientWebSocket ws;
        private object state;
        private string productId;
        private string url;
        private Clients.Kraken client;
        private bool auth;
        private BlockingCollection<byte[]> messageQueue;
        private string bookDepth;


        public KrakenWebsocket(string productId, BlockingCollection<byte[]> messageQueue, object state = null, bool auth = false, int bookDepth = 10)
        {

            this.productId = productId;
            this.messageQueue = messageQueue;
            this.state = state;
            this.client = new Clients.Kraken();
            this.bookDepth = bookDepth.ToString();
            this.auth = auth; // <========================== change here
            url = auth ? "wss://ws-auth.kraken.com" : "wss://ws.kraken.com";

        }



        public Task SendSubscriptionMessage(string channel = "trade", string myToken = "")
        {
            // reformat the productId
            //string formattedProductId = this.productId.Replace("-", "/");
            string baseCurrency = this.productId.Split("-")[0];
            string quoteCurrency = this.productId.Split("-")[1];
            if (baseCurrency == "BTC")
            {
                baseCurrency = "XBT";
            }
            // if (baseCurrency == "ETH")
            // {
            //     baseCurrency = "XETH";
            // }

            string formattedProductId = baseCurrency + "/" + quoteCurrency;
            //Console.WriteLine("KRKN product: {0}", formattedProductId);

            string json = "";

            if (this.auth)
            {
                if (channel == "ownTrades")
                {
                    json = "{\"event\": \"subscribe\", \"subscription\": {\"name\": \"ownTrades\", \"token\": \"" + myToken + "\"}}";
                }
                else if (channel == "openOrders")
                {
                    json = "{\"event\": \"subscribe\", \"subscription\": {\"name\": \"openOrders\", \"token\": \"" + myToken + "\"}}";
                }
            }
            else
            {
                if (channel == "ticker")
                {
                    json = "{\"event\": \"subscribe\", \"pair\": [\"" + formattedProductId + "\"], \"subscription\": {\"name\": \"ticker\"}}";
                }
                else if (channel == "trade")
                {
                    json = "{\"event\": \"subscribe\", \"pair\": [\"" + formattedProductId + "\"], \"subscription\": {\"name\": \"trade\"}}";
                }
                else if (channel == "book")
                {
                    json = "{\"event\": \"subscribe\", \"pair\": [\"" + formattedProductId + "\"], \"subscription\": { \"depth\": " + bookDepth + ", \"name\": \"book\"}}";
                }
            }


            //string json = "{\"type\": \"subscribe\", \"product_ids\": [\"" + this.productId + "\"], \"channels\": [\"level2\", \"ticker\"]}";
            //Console.WriteLine(json);
            var encoded = Encoding.UTF8.GetBytes(json);
            // Define the cancellation token.u
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            var buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);

            // send subscription message
            return ws.SendAsync(buffer, WebSocketMessageType.Text, true, token);
        }

        public static Task SendString(ClientWebSocket ws, String data, CancellationToken cancellation)
        {
            var encoded = Encoding.UTF8.GetBytes(data);
            var buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);
            return ws.SendAsync(buffer, WebSocketMessageType.Text, true, cancellation);
        }

        // Connect to the websocket and start receiving data. Should not return while the 
        public async void Connect()
        {
            CancellationToken token = (CancellationToken)this.state;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    ws = new ClientWebSocket();
                    string krakenToken = this.client.getWebSocketsToken();
                    await ws.ConnectAsync(new Uri(url), CancellationToken.None);
                    // await SendSubscriptionMessage("ownTrades", krakenToken);
                    // await SendSubscriptionMessage("openOrders", krakenToken);

                    if (this.auth)
                    {
                        await SendSubscriptionMessage("ownTrades", krakenToken);
                        await SendSubscriptionMessage("openOrders", krakenToken);
                    }
                    else
                    {
                        await SendSubscriptionMessage("trade", krakenToken);
                        await SendSubscriptionMessage("book", krakenToken);
                        // await SendSubscriptionMessage("ticker", krakenToken);

                    }

                    await Receive();
                }
                catch (Exception e)
                {
                    // Console.WriteLine("KRKN-{0} Websocket closed. {1}", productId, e.ToString());
                    Console.WriteLine("KRKN-{0} Websocket closed.", productId);
                    // Console.WriteLine("Coinbase Websocket closed unexpectedly, with exception: {0}", e.ToString());
                }
                ws.Dispose();
                // sleep for 1 second before trying to reconnect
                Thread.Sleep(2000);
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
                    // if (bytes.Length > 0)
                    // {
                    //     this.messageQueue.Add(bytes);
                    // }
                    this.messageQueue.Add(bytes);
                }
            }
            Console.WriteLine("KRKN ended");
        }


        // /// Add a header to the Websocket HTTP request
        // public void AddHeader(string key, string value)
        // {
        //     ws.Options.SetRequestHeader(key, value);
        // }

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
                    ws.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Websocket() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }


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
