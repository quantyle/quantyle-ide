using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Security.Cryptography;
using System.IO;
using Newtonsoft.Json;
// using System.Globalization;
using System.Threading;


namespace X1.Clients
{

    public class BinanceBookSnapshot
    {
        public long lastUpdateId { get; set; }
        public List<List<string>> bids { get; set; }
        public List<List<string>> asks { get; set; }
    }


    public class BinanceRequests
    {
        public Dictionary<string, string> Headers;
        public Dictionary<string, string> Parameters;
        private HttpClient client = new HttpClient();
        public string apiKey;
        public string unsignedSignature;
        public string passphrase;
        public string Url;

        public BinanceRequests(bool auth = true)
        {
            Headers = new Dictionary<string, string>();
            Parameters = new Dictionary<string, string>();
            this.Url = "https://api.binance.us";
            if (auth)
            {
                Key key = readKeys(); // read keys from file
                this.apiKey = key.key;
                this.unsignedSignature = key.secret;
            }
        }

        public string GenerateSignature(string apiSecret, string message)
        {
            string stringHash;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiSecret)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
                stringHash = BitConverter.ToString(hash).Replace("-", string.Empty);
            }

            return stringHash;
        }


        public static Key readKeys()
        {
            StreamReader r = new StreamReader("keys.json");
            string json = r.ReadToEnd();
            Dictionary<string, Key> keys = JsonConvert.DeserializeObject<Dictionary<string, Key>>(json);
            //Console.WriteLine(keys["GDAX"].passphrase);
            r.Close();
            return keys["BINA"];
        }

        public Task<HttpResponseMessage> Request(string path, HttpMethod method, string data = "", Dictionary<string, string> parameters = null, bool auth = false, bool signed = false)
        {

            string url = this.Url + path;

            // headers
            Dictionary<string, string> headers = new Dictionary<string, string>();
            if (auth) // format auth headers
            {
                headers.Add("X-MBX-APIKEY", this.apiKey);
            }

            // url parameters
            if (parameters != null)
            {
                // add signature to params
                if (signed)
                {
                    string message = parameters.Aggregate(
                        new StringBuilder(),
                        (sb, pair) => sb.AppendFormat("{0}={1}&", pair.Key, pair.Value),
                        (sb) => sb.ToString().TrimEnd('&'));

                    var signedSignature = GenerateSignature(this.unsignedSignature, message);
                    parameters.Add("signature", signedSignature);
                }

                url += "?" + parameters.Aggregate(
                    new StringBuilder(),
                    (sb, pair) => sb.AppendFormat("{0}={1}&", pair.Key, pair.Value),
                    (sb) => sb.ToString()).TrimEnd('&');
            }


            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            if (data != null && data != String.Empty)
                request.Content = new StringContent(data, Encoding.UTF8);
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> p in headers)
                    request.Headers.Add(p.Key, p.Value);
            }

            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            return client.SendAsync(request);
        }

    }



    public class Binance
    {
        public List<string[]> getProducts()
        {
            //string path = String.Format("{0}/api/v3/account", uri);

            string path = "/api/v3/exchangeInfo";
            Dictionary<string, string> param = new Dictionary<string, string>();
            // string timestamp = new DateTimeOffset(DateTime.Now.ToUniversalTime()).ToUnixTimeMilliseconds().ToString();
            // param.Add("timestamp", timestamp);
            // HTTP GET
            BinanceRequests bina = new BinanceRequests();
            var result = bina.Request(path, HttpMethod.Get, parameters: null, auth: true).Result;
            //if (!result.IsSuccessStatusCode) throw new Exception("Bad response");
            // deserialize JSON to object
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            // Console.WriteLine("BINA. getProducts: {0}", jsonContent);
            var response = Newtonsoft.Json.Linq.JObject.Parse(jsonContent);
            List<string[]> products = new List<string[]>();
            foreach (var v in response["symbols"])
            {
                products.Add(new string[]{
                     v["symbol"].ToString(),
                     v["baseAsset"].ToString(),
                     v["quoteAsset"].ToString(),
                });
                // Console.WriteLine(v["symbol"]);
            }
            return products;
        }

        public List<decimal> getTradeFee()
        {
            BinanceAccount binanceAccount = getAccount();
            decimal makerFee = decimal.Parse("0.00" + binanceAccount.makerCommission.ToString());
            decimal takerFee = decimal.Parse("0.00" + binanceAccount.takerCommission.ToString());
            return new List<decimal> { makerFee, takerFee };
        }


        public Dictionary<string, Balance> getBalances()
        {
            Dictionary<string, Balance> balances = new Dictionary<string, Balance>();
            BinanceAccount binanceAccount = getAccount();

            // search backwards since we know USD is towards the end of the list
            for (int i = binanceAccount.balances.Count - 1; i >= 0; i--)
            {
                Dictionary<string, string> item = binanceAccount.balances[i];
                // check if we found "USD"
                // Console.WriteLine("{0}, {1}, {2}", item["asset"].ToString(), item["free"].ToString(), item["locked"].ToString());
                string currency = item["asset"].ToString();
                decimal free = decimal.Parse(item["free"].ToString());
                decimal locked = decimal.Parse(item["locked"].ToString());
                decimal amount = locked + free;
                decimal available = amount;

                if (available > 0 || amount > 0)
                {
                    balances.Add(currency, new Balance
                    {
                        exchange = "GDAX",
                        currency = currency,
                        amount = amount,
                        available = available,
                    });
                }
            }
            return balances;

        }

        public decimal getBalances(string baseCurrency = "USD")
        {
            BinanceAccount binanceAccount = getAccount();

            // search backwards since we know USD is towards the end of the list
            for (int i = binanceAccount.balances.Count - 1; i >= 0; i--)
            {
                Dictionary<string, string> item = binanceAccount.balances[i];
                // check if we found "USD"
                if (item["asset"].ToString() == baseCurrency)
                {
                    return decimal.Parse(item["free"].ToString());
                }
            }
            throw new Exception("missing Binance balance!");
        }

        public BinanceAccount getAccount()
        {
            //string path = String.Format("{0}/api/v3/account", uri);

            string path = "/api/v3/account";
            Dictionary<string, string> param = new Dictionary<string, string>();
            string timestamp = new DateTimeOffset(DateTime.Now.ToUniversalTime()).ToUnixTimeMilliseconds().ToString();
            param.Add("timestamp", timestamp);
            // HTTP GET
            BinanceRequests bina = new BinanceRequests();
            var result = bina.Request(path, HttpMethod.Get, parameters: param, auth: true, signed: true).Result;
            //if (!result.IsSuccessStatusCode) throw new Exception("Bad response");
            // deserialize JSON to object
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            // Console.WriteLine("BINA. getAccount: {0}", jsonContent);
            return System.Text.Json.JsonSerializer.Deserialize<BinanceAccount>(jsonContent);
        }

        public void getAccountSnapshot()
        {
            string path = "/sapi/v1/accountSnapshot";
            Dictionary<string, string> param = new Dictionary<string, string>();
            string timestamp = new DateTimeOffset(DateTime.Now.ToUniversalTime()).ToUnixTimeMilliseconds().ToString();
            param.Add("type", "SPOT");
            param.Add("timestamp", timestamp);
            // HTTP GET
            BinanceRequests bina = new BinanceRequests();
            var result = bina.Request(path, HttpMethod.Get, parameters: param, auth: true, signed: true).Result;
            //if (!result.IsSuccessStatusCode) throw new Exception("Bad response");
            // deserialize JSON to object
            string jsonContent = result.Content.ReadAsStringAsync().Result;

            Console.WriteLine("BINA. getAccountSnapshot: {0}", jsonContent);
        }


        public BinanceBookSnapshot getOrderBookSnapshot(string productId = "BTCUSDC", int limit = 100)
        {
            string path = "/api/v3/depth";
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("symbol", productId);
            param.Add("limit", limit.ToString());

            // HTTP GET
            BinanceRequests bina = new BinanceRequests();
            var result = bina.Request(path, HttpMethod.Get, parameters: param, auth: false).Result;
            // deserialize JSON to object
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            Console.WriteLine("jsonContent {0}", jsonContent);
            BinanceBookSnapshot snapshot = System.Text.Json.JsonSerializer.Deserialize<BinanceBookSnapshot>(jsonContent);
            return snapshot;
        }


        public List<Ticker> getRecentTrades(string productId)
        {
            string path = "/api/v3/trades";
            productId = productId.Replace("-", "");

            // parameters
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("symbol", productId);

            // Console.WriteLine("======================== BINANCE CHART RESPONSE =====================");
            BinanceRequests bina = new BinanceRequests();
            // HTTP GET
            var result = bina.Request(path, HttpMethod.Get, parameters: param, auth: true, signed: false).Result;
            // if (!result.IsSuccessStatusCode) throw new Exception("Bad response");

            // deserialize JSON to object
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            // Console.WriteLine(jsonContent);
            var array = Newtonsoft.Json.Linq.JArray.Parse(jsonContent);
            List<Ticker> trades = new List<Ticker>();

            foreach (var item in array)
            {
                var t = decimal.Parse(item["time"].ToString()) / 1000;
                trades.Add(new Ticker
                {
                    id = Convert.ToDecimal(item["id"]),
                    time = t,
                    price = Convert.ToDecimal(item["price"]),
                    size = Convert.ToDecimal(item["qty"]),
                    side = item["isBuyerMaker"].ToString() == "true" ? "buy" : "sell",
                });
            }
            return trades;
        }
        public List<Ticker> getTrades(string productId, int limit = 0, decimal fromId = 0)
        {
            string path = "/api/v3/historicalTrades";
            productId = productId.Replace("-", "");


            // parameters
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("symbol", productId);
            if (limit > 0)
            {
                param.Add("limit", limit.ToString());
            }
            if (fromId > 0)
            {
                param.Add("fromId", fromId.ToString());
            }

            // Console.WriteLine("======================== BINANCE CHART RESPONSE =====================");
            BinanceRequests bina = new BinanceRequests();
            // HTTP GET
            var result = bina.Request(path, HttpMethod.Get, parameters: param, auth: true, signed: false).Result;
            // if (!result.IsSuccessStatusCode) throw new Exception("Bad response");

            // deserialize JSON to object
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            // Console.WriteLine(jsonContent);

            var array = Newtonsoft.Json.Linq.JArray.Parse(jsonContent);
            List<Ticker> trades = new List<Ticker>();

            foreach (var item in array)
            {
                var t = decimal.Parse(item["time"].ToString()) / 1000;
                trades.Add(new Ticker
                {
                    id = Convert.ToDecimal(item["id"]),
                    time = t,
                    price = Convert.ToDecimal(item["price"]),
                    size = Convert.ToDecimal(item["qty"]),
                    side = item["isBuyerMaker"].ToString() == "true" ? "buy" : "sell",
                });
            }
            return trades;
        }


        public BinanceChartResponse getChartObject(string productId, decimal start = 0, decimal end = 0)
        {
            string path = "/api/v3/klines";

            // parameters
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("symbol", productId);
            param.Add("interval", "1m");

            if (start > 0)
            {
                start *= 1000;
                param.Add("startTime", start.ToString());
                // Console.WriteLine(start.ToString());
            }

            if (end > 0)
            {
                end *= 1000;
                param.Add("endTime", end.ToString());
                // Console.WriteLine(end.ToString());
            }

            // Console.WriteLine("======================== BINANCE CHART RESPONSE =====================");
            BinanceRequests bina = new BinanceRequests();
            // HTTP GET
            var result = bina.Request(path, HttpMethod.Get, parameters: param, auth: false).Result;
            if (!result.IsSuccessStatusCode) throw new Exception("Bad response");

            // deserialize JSON to object
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            // Console.WriteLine(jsonContent);

            return System.Text.Json.JsonSerializer.Deserialize<BinanceChartResponse>("{\"data\": " + jsonContent + "}");
        }

        public List<List<decimal>> getChartList(string productId, decimal start = 0, decimal end = 0)
        {
            //long timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            productId = productId.Replace("-", "");
            BinanceChartResponse chart = getChartObject(productId, start, end);
            List<List<decimal>> chartList = new List<List<decimal>>();

            foreach (var c in chart.data)
            {
                decimal ts = Convert.ToInt64(c[0].ToString()) / 1000;
                // Console.WriteLine(ts);
                chartList.Add(
                    new List<decimal>
                    {
                        ts,
                        Convert.ToDecimal(c[1].ToString()),
                        Convert.ToDecimal(c[2].ToString()),
                        Convert.ToDecimal(c[3].ToString()),
                        Convert.ToDecimal(c[4].ToString()),
                        Convert.ToDecimal(c[5].ToString()),
                    }
                );
                // Console.WriteLine();
                // Console.WriteLine(ts);
                // Console.WriteLine(c[1].ToString());
                // Console.WriteLine(c[2].ToString());
                // Console.WriteLine(c[3].ToString());
                // Console.WriteLine(c[4].ToString());
                // Console.WriteLine(c[5].ToString());
            }
            return chartList;
        }

        public void loadTrades(string product, decimal currentUnixTime, decimal endTime, int delay = 300)
        {
            string path = "./media/data/BINA-" + product + "-trades.csv";
            string pathRev = "./media/data/BINA-" + product + "-trades-rev.csv";
            int limit = 500;
            var trades = getRecentTrades(product);
            var lastId = trades.First().id - limit + 1;
            decimal lastTime = trades.First().time;

            // only start the creation process if path does exist
            if (!File.Exists(path))
            {
                bool reverseFound = File.Exists(pathRev);
                // this means the process was cut short and the last line is likely corrupt
                if (reverseFound)
                {
                    // delete the broken line
                    Files.RemoveLastLine(pathRev);
                    string lastLine = Files.ReadLastLine(pathRev);
                    lastId = decimal.Parse(lastLine.Split(",")[0]) - limit;
                    Console.WriteLine("repairing file for BINA-{0}", product);
                }
                else
                {
                    Console.WriteLine("creating file for BINA-{0}", product);
                }

                // if the file exists we append the text, otherwise we create the file
                using (StreamWriter sw = reverseFound ? File.AppendText(pathRev) : File.CreateText(pathRev))
                {
                    while (true)
                    {
                        trades = getTrades(product, fromId: lastId, limit: limit);
                        for (int i = trades.Count - 1; i > 0; i--)
                        {
                            string line =
                            trades[i].id + "," +
                            trades[i].time + "," +
                            trades[i].side + "," +
                            trades[i].price + "," +
                            trades[i].size;
                            sw.WriteLine(line);
                            // Console.WriteLine("BINA-{0} writing to file: {1}", product, line);
                            lastTime = Convert.ToDecimal(trades[i].time);
                        }

                        // Console.WriteLine("=============");
                        // sw.WriteLine("=============");
                        // Console.WriteLine("mins: {0}", lastTime - endTime);

                        lastId -= limit - 1;

                        // we've gone back in time far enough
                        if (lastTime <= endTime)
                        {
                            break;
                        }

                        Thread.Sleep(delay);
                    }
                }

                Files.RemoveLastLine(pathRev);
                Files.ReverseFile(pathRev, path);
                File.Delete(pathRev);
                Console.WriteLine("{0} created.", path);

            }
            else
            {
                Console.WriteLine("updating file for BINA-{0}", product);

                List<string> lastLines = Files.ReadFileReverse2(path, 4);
                lastId = (int)decimal.Parse(lastLines[1].Split(",")[0]) + 1;
                // StringBuilder sb = new StringBuilder();
                Encoding encoding = Encoding.UTF8;

                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    // go to end of the file, minus the last few bytes
                    stream.Seek(-1 * (lastLines[0].Length), SeekOrigin.End);
                    decimal prevLastTime = -1;
                    while (true)
                    {
                        if (lastTime == prevLastTime)
                        {
                            break;
                        }
                        else
                        {
                            prevLastTime = lastTime;
                        }

                        trades = getTrades(product, fromId: lastId, limit: limit);
                        for (int i = 0; i < trades.Count; i++)
                        {
                            // sb.Append(trades[i].id);
                            // sb.Append(",");
                            // sb.Append(trades[i].time);
                            // sb.Append(",");
                            // sb.Append(trades[i].side);
                            // sb.Append(",");
                            // sb.Append(trades[i].price);
                            // sb.Append(",");
                            // sb.Append(trades[i].size);
                            // sb.AppendLine();
                            string line =
                            trades[i].id + "," +
                            trades[i].time + "," +
                            trades[i].side + "," +
                            trades[i].price + "," +
                            trades[i].size + "\n";
                            stream.Write(encoding.GetBytes(line), 0, encoding.GetByteCount(line));


                            // Console.WriteLine(line);
                            // Console.WriteLine("BINA-{0} writing to file: {1}", product, line);
                            lastTime = Convert.ToDecimal(trades[i].time);
                        }
                        // Console.WriteLine("BINA remaining: {0}", currentUnixTime - lastTime);

                        // sw.Write(sb.ToString());
                        // stream.Write(encoding.GetBytes(sb.ToString()), 0, encoding.GetByteCount(sb.ToString()));
                        // sb.Clear();

                        lastId += limit;


                        // // we've caught up
                        // if (lastTime >= currentUnixTime - 5)
                        // {
                        //     Console.WriteLine("breaking");
                        //     break;
                        // }

                        Thread.Sleep(delay);
                    }
                }
            }
        }

        public void loadPortfolioTrades(
            string[] products,
            decimal startTime,
            decimal endTime)
        {
            foreach (var product in products)
            {
                loadTrades(product, startTime, endTime);
            }
        }

    }

}

// public string updateTrades(string product)
// {
//     // warning! ======= this only works for BINA & GDAX =======
//     string path = "./media/data/BINA-" + product + "-trades.csv"; // reversed 

//     string lastLine = Files.ReadLastLine(path);
//     Console.WriteLine(lastLine);
//     decimal lastId = Convert.ToDecimal(lastLine.Split(",")[0]);
//     Console.WriteLine(lastId);
//     // var lastId = decimal.Parse(trades.First().id) - limit;
//     string line = "";

//     // if the file exists we append the text, otherwise we create the file
//     using (StreamWriter sw = File.AppendText(path))
//     {
//         var trades = getTrades(product, fromId: lastId + 1);
//         int limit = trades.Count;

//         for (int i = 0; i < trades.Count; i++)
//         {
//             line =
//             trades[i].id + "," +
//             trades[i].time + "," +
//             trades[i].side + "," +
//             trades[i].price + "," +
//             trades[i].size;
//             // sw.WriteLine(line);
//             Console.WriteLine(line);
//         }
//         // sw.WriteLine("======");

//         lastId = Convert.ToDecimal(trades.First().id) + limit - 1;
//     }

//     return line;
// }


// public void createChartFile(string productId, decimal start, decimal end, int delay)
// {
//     // @TODO: move the method to a helper class for modifiying static files
//     string path = "./media/data/BINA-" + productId + "-candles.csv";
//     string pathRev = "./media/data/BINA-" + productId + "-candles-rev.csv";
//     decimal timestamp = start;

//     // only start the creation process if path does exist
//     if (!File.Exists(path))
//     {
//         bool reverseFound = File.Exists(pathRev);
//         // this means the process was cut short and the last line is likely corrupt
//         if (reverseFound)
//         {
//             // delete the broken line
//             Files.RemoveLastLine(pathRev);
//             string lastLine = Files.ReadLastLine(pathRev);
//             timestamp = decimal.Parse(lastLine.Split(",")[0]) - 60;
//         }

//         // load the file in reverse order first
//         using (StreamWriter sw = reverseFound ? File.AppendText(pathRev) : File.CreateText(pathRev))
//         {
//             while (true)
//             {
//                 var chart = getChartList(productId, end: (timestamp));
//                 for (var i = chart.Count - 1; i >= 0; i--)
//                 {
//                     string line =
//                     chart[i][0] + "," +
//                     chart[i][1] + "," +
//                     chart[i][2] + "," +
//                     chart[i][3] + "," +
//                     chart[i][4] + "," +
//                     chart[i][5];
//                     sw.WriteLine(line);
//                 }
//                 timestamp = chart.First()[0] - 60;

//                 if (timestamp <= end)
//                 {
//                     break;
//                 }

//                 Thread.Sleep(delay);
//             }
//         }

//         // write the final file
//         using (StreamWriter sw = File.CreateText(path))
//         {
//             Files.ReverseFile(pathRev, path);
//             File.Delete(pathRev);
//             Console.WriteLine("{0} created.", path);
//         }
//     }
//     else
//     {
//         Console.WriteLine("{0} found. skipping creation", path);
//     }
// }



// public void createCharts(
//     string myPortfolio,
//     decimal currentUnixTime,
//     decimal endTime,
//     ProducerConsumerQueue<string> finishedQueue)
// {

//     // load portfolio from file
//     string jsonContent = System.IO.File.ReadAllText(myPortfolio);
//     var portfolio = Utf8Json.JsonSerializer.Deserialize<Dictionary<string, List<string[]>>>(jsonContent);

//     foreach (var product in portfolio["BINA"])
//     {
//         createChartFile(product[0], currentUnixTime, endTime, 300);
//     }
//     finishedQueue.Add("BINA");
// }