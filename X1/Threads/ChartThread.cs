using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;



namespace X1.Threads
{
    class ChartThread
    {
        public static void thread(
            string exchangeId,
            string productId,
            ProducerConsumerQueue<List<decimal>> candlesQueue,
            ConcurrentQueue<List<decimal>> candleSnapshot,
            Func<string, List<List<decimal>>> getChartList,  // callback to GET charts from exchange
            object cts,
            int candleSnapshotSize = 1440, // limit snapshot to past 24 hours
            int updateDelay = 5)
        {
            CancellationToken token = (CancellationToken)cts;
            var nextCandles = new List<List<decimal>>();

            // 1. load the initial chart from API
            var chart = getChartList(productId);

            // 2. add to candleSnapshot queue (for frontend) 
            for (var i = 0; i < chart.Count - 1; i++) candleSnapshot.Enqueue(chart[i]);

            // 3. wait here for the first candle, then throw it away since it may be incomplete
            var candle = candlesQueue.Take();

            // 4. grab the next several candles
            for (var i = 0; i < updateDelay; i++)
            {
                candle = candlesQueue.Take();
                nextCandles.Add(candle);
                candleSnapshot.Enqueue(candle);
            }

            // 5. load the next chart from API
            chart = getChartList(productId);

            // merge the calculated candles with the latest chart
            mergeCharts(productId, exchangeId, chart, nextCandles);

            // clear candles queue, and populate with newly merged list
            candleSnapshot.Clear();
            for (int i = 0; i < chart.Count - 1; i++) candleSnapshot.Enqueue(chart[i]);

            Console.WriteLine("{0}-{1} candles should be up to date. manually reload frontend", exchangeId, productId);

            // for the rest of the lifecycle, update the candles queue
            while (!token.IsCancellationRequested)
            {
                candle = candlesQueue.Take();
                var lastCandle = candleSnapshot.Last();

                // add to candleSnapshot for the frontend
                candleSnapshot.Enqueue(candle);

                while (candleSnapshot.Count > candleSnapshotSize)
                {
                    // trim the candles
                    candleSnapshot.TryDequeue(out List<decimal> throwaway);
                }
            }
        }

        public static void mergeCharts(string productId, string exchangeId, List<List<decimal>> chart1, List<List<decimal>> chart2)
        {
            decimal lastTime = chart1.Last()[0];

            foreach (var c in chart2)
            {
                if (c[0] >= lastTime)
                {
                    chart1.Add(c);
                }
            }

            // try
            // {
            //     // this will throw an error if we have duplicates
            //     var dict = chart1.ToDictionary(p => p[0]);

            //     foreach (var candle in chart2) dict[candle[0]] = candle;
            //     return dict.Values.ToList();
            // }
            // catch (System.ArgumentException e)
            // {
            //     // ----- DUPLICATE CANDLESTICKS EDGE-CASE ON GEMINI-BTC-USD (Dec. 9, 2021 @ ~4:00AM) ----- 
            //     // [GMNI]: 1639079040, 47808.42, 47877.07, 47783.32
            //     // [GMNI]: 1639079100, 47818.46, 47823.25, 47755.67
            //     // [GMNI]: 1639079160, 47773.97, 47809.32, 47733.98  <----- duplicate
            //     // [GMNI]: 1639079160, 47773.97, 47773.97, 47773.97  <----- duplicate
            //     // [GMNI]: 1639079220, 47772.43, 47809.25, 47760
            //     // [GMNI]: 1639079280, 47773.46, 47803.8, 47773.44

            //     string errMessage = "An item with the same key has already been added. Key: ";
            //     if (e.Message.Contains(errMessage))
            //     {
            //         decimal duplicateTimestamp = decimal.Parse(e.Message.Substring(errMessage.Length));
            //         Console.WriteLine(">>> duplicate timestamp: {0}", duplicateTimestamp);
            //         List<List<decimal>> resolved = resolveDuplicates(chart1, duplicateTimestamp);
            //         foreach(var c in resolved){
            //             Console.WriteLine("resolved: {0}", c[0]);
            //         }

            //         //  attempt # 2
            //         var dict = resolved.ToDictionary(p => p[0]);
            //         foreach (var candle in chart2) dict[candle[0]] = candle;
            //         return dict.Values.ToList();
            //     }

            //     return null;
            // }
        }


        // public static List<List<decimal>> resolveDuplicates(List<List<decimal>> chart, decimal duplicateTimestamp)
        // {
        //     List<decimal> candle = new List<decimal>();
        //     List<decimal> nextCandle = new List<decimal>();
        //     List<decimal> mergedCandle = new List<decimal>();

        //     // look for nextCandles
        //     for (var i = 0; i < chart.Count; i++)
        //     {
        //         // check if we found 1 of the nextCandles, 
        //         candle = chart[i];
        //         if (candle[0] == duplicateTimestamp)
        //         {
        //             // the next duplicate  must be at i + 1
        //             nextCandle = chart[i + 1];

        //             // duplicate candle resolution will be as follows:
        //             mergedCandle = new List<decimal>(){
        //                 duplicateTimestamp,
        //                 candle[1], // the first candles open
        //                 candle[2] > nextCandle[2] ? candle[2] : nextCandle[2], // high
        //                 candle[3] < nextCandle[3] ? candle[3] : nextCandle[3], // low
        //                 nextCandle[4], // the last candles close
        //                 candle[5] + nextCandle[5] // sum the volumes
        //             };

        //             chart[i] = mergedCandle;  // replace this candle with merged
        //             chart.RemoveAt(i + 1);  // remove duplicate, as it is no longer needed
        //             return chart;
        //         }
        //     }
        //     return null;
        // }



        public void thread2(
            string exchangeId,
            string productId,
            ProducerConsumerQueue<List<decimal>> candlesQueue,
            ConcurrentQueue<List<decimal>> candles,
            Func<string, List<List<decimal>>> getChartList,  // HTTP get charts callback
            object cts,
            int updateDelay = 5)
        {

            List<List<decimal>> candleList = new List<List<decimal>>();
            CancellationToken token = (CancellationToken)cts;

            // load the initial chart 
            List<List<decimal>> chart = getChartList(productId);
            //Console.WriteLine("chart length: {0}", chart.Count);

            for (var i = 0; i < chart.Count - 1; i++)
            {
                candles.Enqueue(chart[i]);
            };

            Console.WriteLine("last candle in file: {0}", chart.Last()[0]);

            // throw away the first candle
            List<decimal> candle = candlesQueue.Take();

            Console.WriteLine("next candle: {0}", candle[0]);
            // // for 10 minutes, update candles
            // for (var i = 0; i < updateDelay; i++)
            // {
            //     candle = candlesQueue.Take();
            //     candleList.Add(candle);
            //     candles.Enqueue(candle);
            // }

            // // load the next chart (more up-to-date chart)
            // chart = getChartList(productId);
            // // Console.WriteLine(chart);

            // // merge the calculated candles with the latest chart
            // List<List<decimal>> merged = mergeCharts(exchangeId, chart, candleList);

            // // clear candles queue, and populate with newly merged list
            // candles.Clear();
            // for (int i = 0; i < merged.Count - 1; i++) candles.Enqueue(merged[i]);


            // // for the rest of the lifecycle, update the candles queue
            // while (!token.IsCancellationRequested)
            // {
            //     candle = candlesQueue.Take();
            //     candles.Enqueue(candle);
            //     // Console.WriteLine(">>>>>>> GDAX OHLCV");
            //     // Console.WriteLine(candle[0].ToString());
            //     // Console.WriteLine(candle[1].ToString());
            //     // Console.WriteLine(candle[2].ToString());
            //     // Console.WriteLine(candle[3].ToString());
            //     // Console.WriteLine(candle[4].ToString());
            //     // Console.WriteLine(candle[5].ToString());
            //     // Console.WriteLine();
            //     while (candles.Count > 1440)
            //     {
            //         candles.TryDequeue(out List<decimal> throwaway);
            //     }
            // }
        }

    }
}