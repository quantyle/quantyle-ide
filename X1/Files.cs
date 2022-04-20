using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using WebSocketSharp.Server;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
// using System.Diagnostics;
// using Microsoft.AspNetCore.Hosting;
// using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Text;

namespace X1
{
    public static class Files
    {
        public static decimal getCurrentUnixMinute()
        {
            decimal currentUnixTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            currentUnixTime = currentUnixTime - (currentUnixTime % 60);
            return currentUnixTime;
        }

        public static void RemoveLastLine(string path)
        {
            Encoding encoding = Encoding.UTF8;
            byte[] buffer = encoding.GetBytes("\n");
            // get the length of the last line
            string lastLine = Files.ReadLastLine(path, countBlank: true);
            // only remove the link if it's not already a newline
            if (lastLine.Length > 0)
            {
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    // trim the filestream, removing last line
                    stream.SetLength(stream.Length - lastLine.Length - 1);
                    stream.Seek(0, SeekOrigin.End);
                    stream.Write(encoding.GetBytes("\n"), 0, encoding.GetByteCount("\n"));
                }
            }
            // else
            // {
            //     Console.WriteLine("file clean");
            // }
        }

        public static List<string> ReadFileReverse(string path, int length = -1, bool countBlank = false)
        {
            Encoding encoding = Encoding.ASCII;
            int charsize = encoding.GetByteCount("\n");
            byte[] buffer = encoding.GetBytes("\n");
            // char[] chars;
            string rev = "";
            int count = 0;
            List<string> output = new List<string>();

            // begin reading the file
            using (FileStream sr = new FileStream(path, FileMode.Open))
            {
                long endpos = sr.Length;
                string line = "";
                for (long pos = charsize; pos < endpos - charsize; pos += charsize)
                {
                    sr.Seek(-pos, SeekOrigin.End);
                    sr.Read(buffer, 0, buffer.Length);
                    var b = encoding.GetString(buffer);
                    // Console.WriteLine(b);

                    if (b == "\n")
                    {
                        rev = reverseString(line);
                        // Console.WriteLine(rev);
                        // Thread.Sleep(1000);
                        if (rev.Length > 0 || countBlank)
                        {
                            // sw.WriteLine(rev);
                            // Console.WriteLine(rev);
                            output.Add(rev);

                            count++;
                            if (count == length)
                            {
                                break;
                            }
                        }

                        line = "";
                    }
                    else
                    {
                        line += b;
                    }
                }

                // write the last line
                rev = reverseString(line);
                output.Add(rev);

                // Console.WriteLine("done here");
                return output;
            }
        }

        public static string ReadLastLine(string path, bool countBlank = false)
        {
            // 1. read last 3 lines in case 1 or 2 are corrupt
            // 2. check for the last valid line 
            List<string> lastLine = ReadFileReverse(path, 1, countBlank);
            return lastLine[0];
        }

        public static void ReverseFile(string path, string outputPath)
        {
            Encoding encoding = Encoding.ASCII;
            int charsize = encoding.GetByteCount("\n");
            byte[] buffer = encoding.GetBytes("\n");
            char[] chars;
            string rev = "";

            // open both files (one for reading and one for writing)
            using (FileStream sr = new FileStream(path, FileMode.Open))
            using (StreamWriter sw = File.CreateText(outputPath))
            {
                long endpos = sr.Length;
                string line = "";
                for (long pos = charsize; pos < endpos + charsize; pos += charsize)
                {
                    sr.Seek(-pos, SeekOrigin.End);
                    sr.Read(buffer, 0, buffer.Length);
                    var b = encoding.GetString(buffer);
                    // Console.WriteLine(b);

                    if (b == "\n")
                    {
                        chars = line.ToCharArray();
                        Array.Reverse(chars);
                        rev = new string(chars);

                        if (rev.Length > 0)
                        {
                            sw.WriteLine(rev);
                        }

                        line = "";
                    }
                    else
                    {
                        line += b;
                    }
                }

                // writ the last line
                chars = line.ToCharArray();
                Array.Reverse(chars);
                rev = new string(chars);
                Console.WriteLine(rev);
                sw.WriteLine(rev);
            }
        }

        public static Ticker parseTicker(string ticker)
        {
            var items = ticker.Split(",");
            // Console.WriteLine(items[0]);
            // Console.WriteLine(items[1]);
            // Console.WriteLine(items[2]);
            // Console.WriteLine(items[3]);
            // Console.WriteLine(items[4]);

            return new Ticker
            {
                id = decimal.Parse(items[0]),
                time = decimal.Parse(items[1]),
                side = items[2].ToString(),
                price = decimal.Parse(items[3]),
                size = decimal.Parse(items[4])
            };
        }

        public static List<Ticker> ReadLastTicks(string path, int length = -1)
        {
            Encoding encoding = Encoding.ASCII;
            int charsize = encoding.GetByteCount("\n");
            byte[] buffer = encoding.GetBytes("\n");
            char[] chars;
            string rev = "";
            int count = 0;
            List<Ticker> output = new List<Ticker>();

            // begin reading the file
            using (FileStream sr = new FileStream(path, FileMode.Open))
            {
                long endpos = sr.Length;
                string line = "";
                for (long pos = charsize; pos < endpos - charsize; pos += charsize)
                {
                    sr.Seek(-pos, SeekOrigin.End);
                    sr.Read(buffer, 0, buffer.Length);
                    var b = encoding.GetString(buffer);
                    // Console.WriteLine(b);

                    if (b == "\n")
                    {
                        chars = line.ToCharArray();
                        Array.Reverse(chars);
                        rev = new string(chars);
                        // Console.WriteLine(rev);
                        // Thread.Sleep(1000);
                        if (rev.Length > 0)
                        {
                            // sw.WriteLine(rev);
                            // Console.WriteLine(rev);
                            output.Add(parseTicker(rev));
                            count++;
                            if (count == length)
                            {
                                break;
                            }
                        }

                        line = "";
                    }
                    else
                    {
                        line += b;
                    }
                }

                // // write the last line
                // chars = line.ToCharArray();
                // Array.Reverse(chars);
                // rev = new string(chars);
                // output.Add(parseTicker(rev));

                // Console.WriteLine("done here");
                return output;
            }
        }
        public static string reverseString(string line)
        {
            char[] chars = line.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        public static void LoadCandlesFromTicks(string exchange, string product)
        {
            Encoding encoding = Encoding.ASCII;
            int charsize = encoding.GetByteCount("\n");
            byte[] buffer = encoding.GetBytes("\n");
            string rev = "";
            decimal currentUnixTime = Files.getCurrentUnixMinute();

            string basePath = "./media/data/" + exchange + "-" + product;
            string candlespath = basePath + "-candles.csv";
            string candlespathRev = basePath + "-candles-rev.csv";
            string candlespath2rev = basePath + "-candles2-rev.csv";
            string tradespath = basePath + "-trades.csv";

            StringBuilder sb = new StringBuilder();
            var candle =
            new List<decimal>
            {
                0,  // time
                0,  // open
                0,  // high
                0,  // low
                0,  // close
                0   // volume
            };

            // candles.csv file was found
            if (File.Exists(candlespath))
            {

                Files.RemoveLastLine(tradespath);
                Files.RemoveLastLine(candlespath);
                // get last trades.csv time
                // List<string> lastTrades = ReadFileReverse(tradespath, 2);
                // List<string> lastCandles = ReadFileReverse(candlespath, 2);

                // Console.WriteLine(lastTrades[0]);
                // Console.WriteLine(lastTrades[1]);

                // Console.WriteLine("------- waiting -----------");
                // Thread.Sleep(100000);

                string lastTradesLine = Files.ReadLastLine(tradespath);
                Console.WriteLine("last line: {0}", lastTradesLine);
                decimal lastTradeTime = decimal.Parse(lastTradesLine.Split(",")[1]);
                lastTradeTime = lastTradeTime - (lastTradeTime % 60);

                // get last candles.csv time
                string lastCandlesLine = Files.ReadLastLine(candlespath);
                decimal lastCandlesTime = decimal.Parse(lastCandlesLine.Split(",")[0]);

                Console.WriteLine("updating: {0}", candlespath);

                // begin reading the file
                using (FileStream sr = new FileStream(tradespath, FileMode.Open))
                using (StreamWriter sw = File.CreateText(candlespath2rev))
                {
                    string line = "";
                    for (long pos = charsize; pos < sr.Length - charsize; pos += charsize)
                    {
                        sr.Seek(-pos, SeekOrigin.End);
                        sr.Read(buffer, 0, buffer.Length);
                        var b = encoding.GetString(buffer);

                        if (b == "\n")
                        {
                            rev = reverseString(line);
                            // Console.WriteLine(rev);

                            if (rev.Length > 0)
                            {
                                var arr = rev.Split(",");

                                // Console.WriteLine(line);
                                // Console.WriteLine(rev);
                                // Console.WriteLine(buffer.Length);
                                // Console.WriteLine();

                                // 281924980,1644994163.343,b

                                decimal time = Convert.ToDecimal(arr[1]);
                                decimal price = Convert.ToDecimal(arr[3]);

                                decimal ts = time - (time % 60);

                                if (candle[0] == ts)
                                {
                                    // set candlestick "high"
                                    if (price > candle[2])
                                    {
                                        candle[2] = price;
                                    }
                                    // set candlestick "low"
                                    if (price < candle[3])
                                    {
                                        candle[3] = price;
                                    }
                                    // set candlestick "close"
                                    candle[1] = price;  // SET OPEN NOT CLOSE HERE, BECAUSE WE ARE ITERATING BACKWARDS
                                    // increment candlestick "volume"
                                    candle[5] += Convert.ToDecimal(arr[4]);
                                }
                                else
                                {
                                    if (candle[0] > 0)
                                    {
                                        sb.Append(Convert.ToInt64(candle[0]));
                                        sb.Append(",");
                                        sb.Append(candle[1]);
                                        sb.Append(",");
                                        sb.Append(candle[2]);
                                        sb.Append(",");
                                        sb.Append(candle[3]);
                                        sb.Append(",");
                                        sb.Append(candle[4]);
                                        sb.Append(",");
                                        sb.Append(candle[5]);
                                        sw.WriteLine(sb.ToString());
                                        // Console.WriteLine(">> {0}", sb.ToString());
                                        sb.Clear();
                                    }
                                    candle[0] = ts;
                                    candle[1] = price;  // open
                                    candle[2] = price;  // high
                                    candle[3] = price;  // low
                                    candle[4] = price;  // close
                                    candle[5] = decimal.Parse(arr[4]);
                                }

                                // count++;
                                // Console.WriteLine("updating: {0}", (ts - lastCandlesTime));
                                if (ts == lastCandlesTime)
                                {
                                    break;
                                }
                            }
                            line = "";
                        }
                        else
                        {
                            line += b;
                        }
                    }
                }

                // open both files (one for reading and one for writing)
                using (FileStream sr = new FileStream(candlespath2rev, FileMode.Open))
                using (StreamWriter sw = File.AppendText(candlespath))
                {
                    string line = "";
                    for (long pos = charsize; pos < sr.Length + charsize; pos += charsize)
                    {
                        sr.Seek(-pos, SeekOrigin.End);
                        sr.Read(buffer, 0, buffer.Length);
                        var b = encoding.GetString(buffer);

                        if (b == "\n")
                        {
                            rev = reverseString(line);

                            if (rev.Length > 0)
                            {
                                sw.WriteLine(rev);
                            }
                            line = "";
                        }
                        else
                        {
                            line += b;
                        }
                    }

                    // writ the last line
                    rev = reverseString(line);
                    Console.WriteLine(rev);
                    sw.WriteLine(rev);
                }

                File.Delete(candlespath2rev);

                Console.WriteLine("finished updating: {0}", candlespath);
            }
            else
            {

                decimal endTime = Convert.ToInt64(currentUnixTime) - (60 * 60 * 5); // 5 hours
                endTime = endTime - (endTime % 60);

                // begin reading the file
                using (StreamWriter sw = File.CreateText(candlespathRev))
                using (FileStream sr = new FileStream(tradespath, FileMode.Open))
                {
                    string line = "";
                    for (long pos = charsize; pos < sr.Length - charsize; pos += charsize)
                    {
                        sr.Seek(-pos, SeekOrigin.End);
                        sr.Read(buffer, 0, buffer.Length);
                        var b = encoding.GetString(buffer);
                        // Console.WriteLine(b);

                        if (b == "\n")
                        {
                            rev = reverseString(line);
                            if (rev.Length > 0)
                            {
                                var arr = rev.Split(",");
                                decimal time = Convert.ToDecimal(arr[1]);
                                decimal ts = time - (time % 60);
                                decimal price = Convert.ToDecimal(arr[3]);

                                if (candle[0] == ts)
                                {
                                    // set candlestick "high"
                                    if (price > candle[2])
                                    {
                                        candle[2] = price;
                                    }
                                    // set candlestick "low"
                                    if (price < candle[3])
                                    {
                                        candle[3] = price;
                                    }
                                    // set candlestick "close"
                                    candle[1] = price; // SET OPEN NOT CLOSE HERE, BECAUSE WE ARE ITERATING BACKWARDS
                                    // increment candlestick "volume"
                                    candle[5] += Convert.ToDecimal(arr[4]);
                                }
                                else
                                {
                                    if (candle[0] > 0)
                                    {
                                        sb.Append(Convert.ToInt64(candle[0]));
                                        sb.Append(",");
                                        sb.Append(candle[1]);
                                        sb.Append(",");
                                        sb.Append(candle[2]);
                                        sb.Append(",");
                                        sb.Append(candle[3]);
                                        sb.Append(",");
                                        sb.Append(candle[4]);
                                        sb.Append(",");
                                        sb.Append(candle[5]);
                                        sw.WriteLine(sb.ToString());
                                        sb.Clear();
                                    }
                                    candle[0] = ts;
                                    candle[1] = price;
                                    candle[2] = price;
                                    candle[3] = price;
                                    candle[4] = price;
                                    candle[5] = Convert.ToDecimal(arr[4]);

                                    Console.WriteLine(ts - endTime);
                                    if (ts == endTime)
                                    {
                                        break;
                                    }
                                }
                            }

                            line = "";
                        }
                        else
                        {
                            line += b;
                        }
                    }
                }

                ReverseFile(candlespathRev, candlespath);
                File.Delete(candlespathRev);
            }
            // Console.WriteLine("done loading from ticks");
        }

        public static List<string> ReadFileReverse2(string path, int length = -1)
        {
            Encoding encoding = Encoding.ASCII;
            int charsize = encoding.GetByteCount("\n");
            byte[] buffer = encoding.GetBytes("\n");
            string rev = "";
            int count = 0;
            List<string> output = new List<string>();

            // begin reading the file
            using (FileStream sr = new FileStream(path, FileMode.Open))
            {
                long endpos = sr.Length;
                string line = "";
                for (long pos = charsize; pos < endpos - charsize; pos += charsize)
                {
                    sr.Seek(-pos, SeekOrigin.End);
                    sr.Read(buffer, 0, buffer.Length);
                    var b = encoding.GetString(buffer);
                    // Console.WriteLine(b);

                    if (b == "\n")
                    {
                        rev = reverseString(line);
                        // Console.WriteLine(rev);
                        // Thread.Sleep(1000);
                        if (rev.Length > 0)
                        {
                            // sw.WriteLine(rev);
                            // Console.WriteLine(rev);
                            output.Add(rev);
                        }
                        else
                        {
                            output.Add("");
                        }

                        count++;
                        if (count == length)
                        {
                            break;
                        }

                        line = "";
                    }
                    else
                    {
                        line += b;
                    }
                }

                // write the last line
                rev = reverseString(line);
                output.Add(rev);

                // Console.WriteLine("done here");
                return output;
            }
        }


    }
}