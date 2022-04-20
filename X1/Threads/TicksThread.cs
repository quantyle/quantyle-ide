using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;


namespace X1.Threads
{
    class TicksThread
    {
        public List<List<decimal>> resolveDuplicates(List<List<decimal>> chart, decimal duplicateTimestamp)
        {
            List<decimal> candle = new List<decimal>();
            List<decimal> nextCandle = new List<decimal>();
            List<decimal> mergedCandle = new List<decimal>();

            // look for nextCandles
            for (var i = 0; i < chart.Count; i++)
            {
                // check if we found 1 of the nextCandles, 
                candle = chart[i];
                if (candle[0] == duplicateTimestamp)
                {
                    // the next duplicate  must be at i + 1
                    nextCandle = chart[i + 1];

                    // duplicate candle resolution will be as follows:
                    mergedCandle = new List<decimal>(){
                        duplicateTimestamp,
                        candle[1], // the first candles open
                        candle[2] > nextCandle[2] ? candle[2] : nextCandle[2], // high
                        candle[3] < nextCandle[3] ? candle[3] : nextCandle[3], // low
                        nextCandle[4], // the last candles close
                        candle[5] + nextCandle[5] // sum the volumes
                    };

                    chart[i] = mergedCandle;  // replace this candle with merged
                    chart.RemoveAt(i + 1);  // remove duplicate, as it is no longer needed
                    return chart;
                }
            }
            return null;
        }

        public List<List<decimal>> mergeCharts(string exchangeId, List<List<decimal>> chart1, List<List<decimal>> chart2)
        {
            try
            {
                // this will throw an error if we have duplicates
                var dict = chart1.ToDictionary(p => p[0]);
                foreach (var candle in chart2) dict[candle[0]] = candle;
                return dict.Values.ToList();
            }
            catch (System.ArgumentException e)
            {
                // ----- DUPLICATE CANDLESTICKS EDGE-CASE ON GEMINI-BTC-USD (Dec. 9, 2021 @ ~4:00AM) ----- 
                // [GMNI]: 1639079040, 47808.42, 47877.07, 47783.32
                // [GMNI]: 1639079100, 47818.46, 47823.25, 47755.67
                // [GMNI]: 1639079160, 47773.97, 47809.32, 47733.98  <----- duplicate
                // [GMNI]: 1639079160, 47773.97, 47773.97, 47773.97  <----- duplicate
                // [GMNI]: 1639079220, 47772.43, 47809.25, 47760
                // [GMNI]: 1639079280, 47773.46, 47803.8, 47773.44

                string errMessage = "An item with the same key has already been added. Key: ";
                if (e.Message.Contains(errMessage))
                {
                    decimal duplicateTimestamp = decimal.Parse(e.Message.Substring(errMessage.Length));
                    List<List<decimal>> resolved = resolveDuplicates(chart1, duplicateTimestamp);

                    //  attempt # 2
                    var dict = resolved.ToDictionary(p => p[0]);
                    foreach (var candle in chart2) dict[candle[0]] = candle;
                    return dict.Values.ToList();
                }
                Console.WriteLine("unexpected error merging {0} charts", exchangeId);
                return null;
            }
        }


        // 1. get ticks from ticksQueue
        // 2. save if interval has passed
        
        public void threadV2(
            string exchange,
            string product,
            ProducerConsumerQueue<Ticker> ticksQueue,
            ConcurrentQueue<Ticker> tickSnapshot,
            object cts,
            int updateDelay = 5)
        {
            CancellationToken token = (CancellationToken)cts;
            Clients.Coinbase coinbaseCli = new Clients.Coinbase();
            string path = "./media/data/" + exchange + "-" + product + "-trades.csv"; // reversed 
            var lastTicks = Files.ReadLastTicks(path, 20);
            int snapshotSize = 200;
            int updateFreqSeconds = 5;
            int ohlcvResolution = 60;
            lastTicks.Reverse();
            List<decimal> ohlcv = new List<decimal>();
            StringBuilder stringBuilder = new StringBuilder();
            decimal lastId = lastTicks.Last().id - 1;
            decimal timestamp = lastTicks.Last().time;
            decimal lastSaveTime = timestamp;
            List<Ticker> ticks = new List<Ticker>();
            StringBuilder sb = new StringBuilder();
            List<decimal> candle = new List<decimal>();


            
        }


        public void thread(
            string exchange,
            string product,
            ProducerConsumerQueue<Ticker> ticksQueue,
            ConcurrentQueue<Ticker> tickSnapshot,
            object cts,
            int updateDelay = 5)
        {
            CancellationToken token = (CancellationToken)cts;
            Clients.Coinbase coinbaseCli = new Clients.Coinbase();
            string path = "./media/data/" + exchange + "-" + product + "-trades.csv"; // reversed 
            var lastTicks = Files.ReadLastTicks(path, 20);
            int snapshotSize = 200;
            int updateFreqSeconds = 5;
            int ohlcvResolution = 60;
            lastTicks.Reverse();
            List<decimal> ohlcv = new List<decimal>();
            StringBuilder stringBuilder = new StringBuilder();
            decimal lastId = lastTicks.Last().id - 1;
            decimal timestamp = lastTicks.Last().time;
            decimal lastSaveTime = timestamp;
            List<Ticker> ticks = new List<Ticker>();
            StringBuilder sb = new StringBuilder();
            List<decimal> candle = new List<decimal>();


            // populate snapshot queue
            foreach (var item in lastTicks)
            {
                tickSnapshot.Enqueue(item);
                decimal ts = item.time - (item.time % ohlcvResolution);
                if (candle.Count > 0)
                {
                    if (ts == candle[0])
                    {
                        // set candlestick "high"
                        if (item.price > candle[2])
                        {
                            candle[2] = item.price;
                        }
                        // set candlestick "low"
                        if (item.price < candle[3])
                        {
                            candle[3] = item.price;
                        }
                        // set candlestick "close"
                        candle[4] = item.price;
                        // increment candlestick "volume"
                        candle[5] += item.size;
                    }
                    else
                    {
                        // Console.WriteLine("---------- candle: {0}", ts);
                        // write candle to file
                        candle =
                            new List<decimal>
                            {
                                ts,
                                item.price, // open
                                item.price, // high
                                item.price, // low
                                item.price,  // close
                                item.size, // volume
                            };
                    }
                }
                else
                {
                    candle =
                        new List<decimal>
                        {
                            ts,
                            item.price,
                            item.price,
                            item.price,
                            item.price,
                            item.size,
                        };
                }
            }


            // for the rest of the lifecycle, update the candles queue
            using (StreamWriter sw = File.AppendText(path))
            {
                while (!token.IsCancellationRequested)
                {

                    Ticker tick = ticksQueue.Take();
                    timestamp = tick.time;

                    if (tick.id == (lastId + 1))  // this is the next tick in the sequence
                    {
                        tickSnapshot.Enqueue(tick);
                        ticks.Add(tick);
                        lastId = tick.id;
                        // Console.WriteLine(tick.id);

                        // trim snapshot NOT the lastTicks
                        if (tickSnapshot.Count > snapshotSize)
                        {
                            tickSnapshot.TryDequeue(out Ticker throwaway);
                        }

                        // Console.WriteLine("...");

                        // save to file
                        if (timestamp > (lastSaveTime + updateFreqSeconds))
                        {
                            // Console.WriteLine("writing to {0}-{1}-trades.csv", exchange, product);
                            
                            
                            // Console.WriteLine("{0} {1}", product, lastTicks.First().time);
                            lastSaveTime = timestamp;
                            decimal ts = timestamp - (timestamp % ohlcvResolution);

                            foreach (var item in ticks)
                            {
                                // update file
                                sb.Append(item.id);
                                sb.Append(",");
                                sb.Append(item.time);
                                sb.Append(",");
                                sb.Append(item.side);
                                sb.Append(",");
                                sb.Append(item.price);
                                sb.Append(",");
                                sb.Append(item.size);
                                sb.AppendLine();


                                if (candle.Count > 0)
                                {
                                    if (ts == candle[0])
                                    {
                                        // set candlestick "high"
                                        if (item.price > candle[2])
                                        {
                                            candle[2] = item.price;
                                        }
                                        // set candlestick "low"
                                        if (item.price < candle[3])
                                        {
                                            candle[3] = item.price;
                                        }
                                        // set candlestick "close"
                                        candle[4] = item.price;
                                        // increment candlestick "volume"
                                        candle[5] += item.size;
                                    }
                                    else
                                    {
                                        // Console.WriteLine("---------- candle: {0}", ts);
                                        // write candle to file
                                        candle =
                                            new List<decimal>
                                            {
                                                ts,
                                                item.price, // open
                                                item.price, // high
                                                item.price, // low
                                                item.price,  // close
                                                item.size, // volume
                                            };
                                    }
                                }
                                else
                                {
                                    candle =
                                        new List<decimal>
                                        {
                                            ts,
                                            item.price,
                                            item.price,
                                            item.price,
                                            item.price,
                                            item.size,
                                        };
                                }


                            }
                            // write to file
                            sw.Write(sb.ToString()); //  ============ UNCOMMENT ============
                            sb.Clear();

                            ticks.Clear();
                        }
                    }
                    else if (tick.id > (lastId + 1)) // there is a gap in the sequence
                    {
                        // theres a gap in the sequence
                        var trades = coinbaseCli.getTrades(product, before: lastId + 1);

                        // write to file
                        for (int i = trades.Count - 1; i > 0; i--)
                        {
                            if (trades[i].id <= tick.id) // BUG HERE
                            {
                                tickSnapshot.Enqueue(trades[i]);
                                ticks.Add(trades[i]);

                                // trim snapshot NOT the lastTicks
                                if (tickSnapshot.Count > snapshotSize)
                                {
                                    tickSnapshot.TryDequeue(out Ticker throwaway);
                                }
                            }
                        }
                        lastId = ticks.Last().id;

                        Thread.Sleep(300);
                    }

                    // ================ calculate candlestick ========================== 
                }
            }
        }
    }
}