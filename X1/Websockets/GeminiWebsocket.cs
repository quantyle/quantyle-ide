
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.IO;
using System.Collections.Concurrent;


namespace X1.Websockets
{
    public class GeminiWebsocket : IDisposable
    {
        private int BufferSize = 512;
        private ClientWebSocket ws;
        private string url;
        private BlockingCollection<byte[]> messageQueue;
        private object state;
        private string productId;
        private int count;

        // Create a new websocket pointing to a server.
        public GeminiWebsocket(string productId, BlockingCollection<byte[]> messageQueue, object state = null)
        {
            this.productId = productId;
            //ws.Options.SetBuffer(BufferSize, 17000);
            this.url = "wss://api.gemini.com/v1/marketdata/" + productId.Replace("-", "");
            this.state = state;
            this.messageQueue = messageQueue;
            this.count = 0;
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
                    await ws.ConnectAsync(new Uri(url), CancellationToken.None);
                    //await SendSubscriptionMessage();
                    await Receive();
                }
                catch (System.Net.WebSockets.WebSocketException)
                {
                    Console.WriteLine("GMNI-{0} Websocket closed.", productId);
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
                    if (bytes.Length > 0)
                    {
                        this.messageQueue.Add(bytes);
                    }
                }
            }
        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        // Dispose Websocket
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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    // the code below may be needed for Gemini API version 2 (v2)

    // public string ComputeSignature(
    //     HttpMethod httpMethod,
    //     string secret,
    //     double timestamp,
    //     string requestUri,
    //     string contentBody = "")
    // {
    //     var convertedString = Convert.FromBase64String(secret);
    //     var prehash = timestamp.ToString("F0", CultureInfo.InvariantCulture) + httpMethod.ToString().ToUpper() + requestUri + contentBody;
    //     return HashString(prehash, convertedString);
    // }

    // private string HashString(string str, byte[] secret)
    // {
    //     var bytes = Encoding.UTF8.GetBytes(str);
    //     using (var hmaccsha = new HMACSHA256(secret))
    //     {
    //         return Convert.ToBase64String(hmaccsha.ComputeHash(bytes));
    //     }
    // }

    // public static Key readKeys()
    // {

    //     StreamReader r = new StreamReader("keys.json");
    //     string json = r.ReadToEnd();
    //     Dictionary<string, Key> keys = JsonConvert.DeserializeObject<Dictionary<string, Key>>(json);
    //     Console.WriteLine(keys["GDAX"].passphrase);
    //     r.Close();
    //     return keys["GDAX"];

    // }


    // public Task SendSubscriptionMessage()
    // {

    //     Key key = readKeys();

    //     long timestamp = DateTime.UtcNow.ToTimestamp();

    //     //'{"type": "subscribe","subscriptions":[{"name":"l2","symbols":["BTCUSD","ETHUSD","ETHBTC"]}]}'

    //     string json = "{\"type\": \"subscribe\", \"subcriptions\": [{\"name\": \"l2\", \"symbols\": [\"" + this.productId.Replace("-", "") + "\"]}]}";
    //     Console.WriteLine(json);
    //     var encoded = Encoding.UTF8.GetBytes(json);
    //     // Define the cancellation token.
    //     CancellationTokenSource source = new CancellationTokenSource();
    //     CancellationToken token = source.Token;
    //     var buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);
    //     return ws.SendAsync(buffer, WebSocketMessageType.Text, true, token);
    // }

    // public static Task SendString(ClientWebSocket ws, String data, CancellationToken cancellation)
    // {
    //     var encoded = Encoding.UTF8.GetBytes(data);
    //     var buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);
    //     return ws.SendAsync(buffer, WebSocketMessageType.Text, true, cancellation);
    // }
}
