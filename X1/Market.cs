using System;
using System.Collections.Generic;
using System.Linq;
using NetMQ;
using NetMQ.Sockets;

namespace X1
{
    public class Market
    {

        public SortedDictionary<decimal, decimal> asks = new SortedDictionary<decimal, decimal>();
        public SortedDictionary<decimal, decimal> bids = new SortedDictionary<decimal, decimal>();
        public List<Ticker> tickers = new List<Ticker>();
        public UIPayload payload = new UIPayload();
        public PublisherSocket pubSocket = new PublisherSocket();
        long lastUpdateTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

        int updateFreqMilliseconds;
        string threadKey;
        public Market(int updateFreqMilliseconds, string threadKey)
        {
            this.updateFreqMilliseconds = updateFreqMilliseconds;
            this.threadKey = threadKey;
            // connect NetMQ socket
            this.pubSocket.Options.SendHighWatermark = 25;  // REMOVE THIS AND TEST FOR PERFORMANCE
            this.pubSocket.Bind("inproc://" + threadKey);

        }

        public void updateUI()
        {

            long currentUpdateTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            if (currentUpdateTime > (lastUpdateTime + updateFreqMilliseconds) && asks.Count > 10)
            {
                payload.book.volume = 0;
                var c = payload.book.asks.GetLength(0) - 1;
                for (int i = 0; i <= c; i++)
                {
                    // asks
                    var a = asks.ElementAt(i);
                    payload.book.asks[c - i, 0] = a.Key; // price
                    payload.book.asks[c - i, 1] = a.Value; // volume
                                                           // bids
                    var b = bids.ElementAt(bids.Count - i - 1);
                    payload.book.bids[i, 0] = b.Key;  // price
                    payload.book.bids[i, 1] = b.Value;  // volume
                                                        // sum volumes
                    payload.book.volume += (b.Value + a.Value);
                }

                payload.tickers = tickers;
                byte[] result = Utf8Json.JsonSerializer.Serialize(payload);
                pubSocket.SendMoreFrame(threadKey).SendFrame(result);
                lastUpdateTime = currentUpdateTime;

                // empty/reset tickers
                tickers.Clear();

            }


            // Console.WriteLine("UIFeed.OnMessage");

        }

    }



}