using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Globalization;
using System.Security.Cryptography;
using System.IO;
using Newtonsoft.Json;
using System.Threading;


namespace X1.Clients
{


    public class GeminiRequests
    {
        public string Data;
        public Dictionary<string, string> Headers;
        public Dictionary<string, string> Parameters;
        private HttpClient client = new HttpClient();
        public string apiKey;
        public string unsignedSignature;
        public string uri;

        public GeminiRequests()
        {
            Headers = new Dictionary<string, string>();
            Parameters = new Dictionary<string, string>();
            this.uri = "https://api.gemini.com";
            this.Data = "";
            Key keys = readKeys(); // read keys from file
            this.apiKey = keys.key;
            this.unsignedSignature = keys.secret;
        }

        private Uri GenerateUri()
        {
            return new Uri(this.uri +
                this.Parameters.Aggregate(
                    new StringBuilder(),
                    (sb, pair) => sb.AppendFormat("{0}={1}&", pair.Key, pair.Value),
                    (sb) => sb.ToString()));
        }


        public string ComputeSignature(
            HttpMethod httpMethod,
            string secret,
            double timestamp,
            string requestUri,
            string contentBody = "application/json")
        {
            var convertedString = Convert.FromBase64String(secret);
            var prehash = timestamp.ToString("F0", CultureInfo.InvariantCulture) + httpMethod.ToString().ToUpper() + requestUri + contentBody;
            return HashString(prehash, convertedString);
        }

        private string HashString(string str, byte[] secret)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            using (var hmaccsha = new HMACSHA256(secret))
            {
                return Convert.ToBase64String(hmaccsha.ComputeHash(bytes));
            }
        }

        public Key readKeys()
        {
            StreamReader r = new StreamReader("keys.json");
            string json = r.ReadToEnd();
            Dictionary<string, Key> keys = JsonConvert.DeserializeObject<Dictionary<string, Key>>(json);
            //Console.WriteLine(keys["GMNI"].passphrase);
            r.Close();
            return keys["GMNI"];
        }

        public Task<HttpResponseMessage> Request(string path, HttpMethod method, string data = "", Dictionary<string, string> parameters = null, bool auth = false)
        {
            string url = uri + path;
            //Console.WriteLine(url);
            if (parameters != null)
            {
                url += "?" + parameters.Aggregate(
                 new StringBuilder(),
                 (sb, pair) => sb.AppendFormat("{0}={1}&", pair.Key, pair.Value),
                 (sb) => sb.ToString());
                //Console.WriteLine("GET: " + url);
            }

            Dictionary<string, string> headers = new Dictionary<string, string>();
            if (auth) // format auth headers
            {
                var timestamp = (DateTime.UtcNow.ToTimestamp() * 1000).ToString();

                // if we use 1 second resolution timestamp for the nonce, we would not be able to send API calls within
                // the same second, so we use milliseconds timestamp
                //string timestamp = new DateTimeOffset(DateTime.Now.ToUniversalTime()).ToUnixTimeMilliseconds().ToString();

                string plain = "{\"request\": \"" + path + "\", \"nonce\": \"" + timestamp + "\"}";
                string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(plain));
                string signature = Cryptography.SHA384Sign(base64, this.unsignedSignature);

                // Console.WriteLine(plain);
                // Console.WriteLine(base64);

                //Thread.Sleep(100000);
                //headers.Add("Content-Length", "0");
                headers.Add("X-GEMINI-APIKEY", this.apiKey);
                headers.Add("X-GEMINI-PAYLOAD", base64);
                headers.Add("X-GEMINI-SIGNATURE", signature);
            }

            HttpRequestMessage request = new HttpRequestMessage(method, url);
            if (data != null && data != String.Empty)
                request.Content = new StringContent(data, Encoding.UTF8);
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> p in headers)
                    request.Headers.Add(p.Key, p.Value);
            }

            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));

            return client.SendAsync(request);
        }
    }



    public class Gemini
    {
        public List<Ticker> getTrades(string productId = "BTC-USD", decimal since = 0)
        {
            string symbol = productId.Replace("-", "").ToLower();
            string path = "/v1/trades/" + symbol;
            if (since > 0)
            {
                path += ("?limit_trades=500&timestamp=" + since.ToString());
            }

            GeminiRequests gemini = new GeminiRequests();
            // HTTP GET
            var result = gemini.Request(path, HttpMethod.Get, auth: true).Result;
            if (!result.IsSuccessStatusCode) throw new Exception("Bad response");
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            // Console.WriteLine(jsonContent);
            var array = Newtonsoft.Json.Linq.JArray.Parse(jsonContent);
            List<Ticker> trades = new List<Ticker>();
            //  return System.Text.Json.JsonSerializer.Deserialize<ChartResponse>("{\"data\": " +  + "}");
            foreach (var item in array)
            {
                var t = decimal.Parse(item["timestampms"].ToString()) / 1000;
                // Console.WriteLine(t);
                trades.Add(new Ticker
                {
                    id = Convert.ToDecimal(item["tid"]),
                    time = t,
                    price = Convert.ToDecimal(item["price"]),
                    size = Convert.ToDecimal(item["amount"]),
                    side = item["type"].ToString(),
                });
            }
            return trades;
        }


        public List<List<decimal>> getChartList(string productId = "BTC-USD")
        {
            string symbol = productId.Replace("-", "").ToLower();
            string url = "/v2/candles/" + symbol + "/1m";

            GeminiRequests gemini = new GeminiRequests();
            // HTTP GET
            var result = gemini.Request(url, HttpMethod.Get, auth: true).Result;
            if (!result.IsSuccessStatusCode) throw new Exception("Bad response");
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            //Console.WriteLine(jsonContent);

            ChartResponse chart = System.Text.Json.JsonSerializer.Deserialize<ChartResponse>("{\"data\": " + jsonContent + "}");
            List<List<decimal>> chartList = new List<List<decimal>>();

            foreach (var c in chart.data)
            {
                chartList.Insert(0,
                    new List<decimal>
                    {
                        Convert.ToDecimal((Convert.ToInt64(c[0]) / 1000)),
                        c[1],
                        c[2],
                        c[3],
                        c[4],
                        c[5],
                    }
                );
            }
            return chartList;
        }





        public void updateProducts()
        {
            string path = "/v1/symbols";

            GeminiRequests gemini = new GeminiRequests();
            // HTTP GET
            var result = gemini.Request(path, HttpMethod.Get, auth: true).Result;
            //if (!result.IsSuccessStatusCode) throw new Exception("Bad response");
            string jsonContent = result.Content.ReadAsStringAsync().Result;

            Console.WriteLine("Gemini.getProducts: {0}", jsonContent);

            var symbols = jsonContent.Replace("\"", "").Replace("[", "").Replace("]", "").Split(",");
            Console.WriteLine(symbols[0]);
            foreach (var symbol in symbols)
            {
                // HTTP GET: product details
                var res = gemini.Request("/v1/symbols/details/" + symbol, HttpMethod.Get, auth: true).Result;
                var data = res.Content.ReadAsStringAsync().Result;
                // example response
                // {
                //     "symbol": "BTCGUSD",
                //     "base_currency": "BTC",
                //     "quote_currency": "GUSD",
                //     "tick_size": 1E-8,
                //     "quote_increment": 0.01,
                //     "min_order_size": "0.00001",
                //     "status": "open",
                //     "wrap_enabled": false
                // },
                using (StreamWriter w = File.AppendText("geminiSymbols.json"))
                {
                    w.WriteLine(data + ",");
                }
                Thread.Sleep(300);
            }


            // if (response["result"].ToString() != "error")
            // {
            //     Console.WriteLine(response[0]);
            // }
            // else
            // {
            //     // @TODO =================================== handle error edge-cases
            //     // e.g. we cannot get initial products because gemini is under maintenance, 
            //     // so we should get saved products from file instead
            //     var reason = response["reason"];
            //     var message = response["message"];
            // }
        }

        public List<string[]> getProducts()
        {
            // read the products from file (save time over getting each /v1/symbols/details/:symbol)
            string jsonContent = System.IO.File.ReadAllText("geminiSymbols.json");
            var response = Newtonsoft.Json.Linq.JObject.Parse(jsonContent);
            List<string[]> products = new List<string[]>();
            foreach (var item in response["symbols"])
            {
                products.Add(
                    new string[]{
                        item["symbol"].ToString(),
                        item["base_currency"].ToString(),
                        item["base_currency"].ToString(),
                    }
                );
            }
            return products;
        }

        public Dictionary<string, Balance> getBalances()
        {
            string path = "/v1/balances";

            GeminiRequests gemini = new GeminiRequests();
            // HTTP GET
            var result = gemini.Request(path, HttpMethod.Post, auth: true).Result;
            //if (!result.IsSuccessStatusCode) throw new Exception("Bad response");
            string jsonContent = result.Content.ReadAsStringAsync().Result;

            // Console.WriteLine("Gemini.getBalances: {0}", jsonContent);

            Dictionary<string, Balance> balances = new Dictionary<string, Balance>();

            var currencyList = JsonConvert.DeserializeObject<List<GeminiBalance>>(jsonContent);
            foreach (var item in currencyList)
            {
                balances.Add(item.currency, new Balance
                {
                    exchange = "GMNI",
                    currency = item.currency.ToString(),
                    amount = decimal.Parse(item.amount),  // amount
                    available = decimal.Parse(item.available)  // available
                });
            }
            return balances;
        }

        public decimal getBalance(string baseCurrency = "USD")
        {
            string path = "/v1/balances";

            GeminiRequests gemini = new GeminiRequests();
            // HTTP GET
            var result = gemini.Request(path, HttpMethod.Post, auth: true).Result;
            //if (!result.IsSuccessStatusCode) throw new Exception("Bad response");
            string jsonContent = result.Content.ReadAsStringAsync().Result;

            Console.WriteLine("Gemini.getBalances: {0}", jsonContent);


            var currencyList = JsonConvert.DeserializeObject<List<GeminiBalance>>(jsonContent);
            for (int i = 0; i < currencyList.Count; i++)
            {
                var currency = currencyList[i];
                if (currency.currency == baseCurrency)
                {
                    return decimal.Parse(currency.amount);
                }
            }
            throw new Exception("missing Gemini balance!");
        }


        public List<decimal> getFees(string baseCurrency = "USD")
        {
            string path = "/v1/notionalvolume";

            GeminiRequests gemini = new GeminiRequests();
            // HTTP GET
            var result = gemini.Request(path, HttpMethod.Post, auth: true).Result;
            if (!result.IsSuccessStatusCode) throw new Exception("Bad response");

            string jsonContent = result.Content.ReadAsStringAsync().Result;

            GeminiFees fees = Utf8Json.JsonSerializer.Deserialize<GeminiFees>(jsonContent);

            // Gemini seems to have a bug, where new accounts with no volume return a maker/taker fee of 0. 
            // This is incorrect, the lowest fee is 10 BPS as of 2021.
            if (fees.api_maker_fee_bps == 0)
            {
                fees.api_maker_fee_bps = 10;
            }
            if (fees.api_taker_fee_bps == 0)
            {
                fees.api_taker_fee_bps = 35;
            }

            decimal makerFee = decimal.Parse("0.00" + fees.api_maker_fee_bps.ToString());
            decimal takerFee = decimal.Parse("0.00" + fees.api_taker_fee_bps.ToString());

            return new List<decimal> { makerFee, takerFee };
        }



        public void loadTrades(string product, decimal currentUnixTime, decimal endTime, int delay = 300)
        {
            decimal currentUnixTimeMS = currentUnixTime * 1000;
            string path = "./media/data/GMNI-" + product + "-trades.csv"; // reversed 
            // decimal endTime = Convert.ToInt64(currentUnixTime) - (60 * 60 * 24 * days);
            // decimal endTime = Convert.ToInt64(currentUnixTime) - (60 * 60 * 6);
            // var lastId = decimal.Parse(trades.First().id) - limit;
            bool fileFound = File.Exists(path);
            Encoding encoding = Encoding.UTF8;

            // if the file exists, update it
            if (fileFound)
            {
                // remove the last line and get the last timestamp
                Files.RemoveLastLine(path);

                List<string> lastLines = Files.ReadFileReverse2(path, 30);
                // endTime = (int)decimal.Parse(lastLines[1].Split(",")[1]);
                decimal endTimeMS = Math.Round((decimal.Parse(lastLines[1].Split(",")[1]) * 1000), 0);
                int delta = 0;
                for (var i = 0; i < lastLines.Count; i++)
                {
                    var lastLine = lastLines[i];
                    if (lastLine.Split(",").Length == 5)
                    {
                        if (Math.Round((decimal.Parse(lastLine.Split(",")[1]) * 1000), 0) != endTimeMS)
                        {
                            endTimeMS = Math.Round((decimal.Parse(lastLine.Split(",")[1]) * 1000), 0);
                            break;
                        }
                    }
                    delta += lastLine.Length + 1;
                }

                // if the file exists we append the text, otherwise we create the file
                // using (StreamWriter sw = File.AppendText(path))
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    Console.WriteLine("updating file for GMNI-{0}", product);
                    stream.Seek(-1 * (delta - 1), SeekOrigin.End); // overwrite the last few lines that have the same MS
                    decimal prevLastTime = -1;

                    while (true)
                    {
                        Console.WriteLine("updating file for GMNI-{0}... {1}", product, (currentUnixTimeMS - endTimeMS));
                        var trades = getTrades(product, since: endTimeMS);

                        // break if we see the same time again (the API gives no new updates)
                        if (endTimeMS == prevLastTime)
                        {
                            break;
                        }
                        else
                        {
                            prevLastTime = endTimeMS;
                        }

                        decimal currTime = 0;
                        for (int i = trades.Count - 1; i > 0; i--)
                        {
                            currTime = Math.Round((trades[i].time * 1000), 0);

                            if (currTime > endTimeMS)
                            {
                                string line =
                                trades[i].id + "," +
                                trades[i].time + "," +
                                trades[i].side + "," +
                                trades[i].price + "," +
                                trades[i].size + "\n";

                                stream.Write(encoding.GetBytes(line), 0, encoding.GetByteCount(line));
                            }
                        }

                        endTimeMS = currTime;

                        Thread.Sleep(delay * 3);
                    }
                }
            }
            else
            {
                // if the file is not found, 
                Console.WriteLine("creating file for GMNI-{0}", product);
                // if the file exists we append the text, otherwise we create the file
                using (StreamWriter sw = fileFound ? File.AppendText(path) : File.CreateText(path))
                {
                    while (true)
                    {
                        // trades = binanceCli.getTrades("BTC-USD", fromId: lastId + 1).data;
                        var trades = getTrades(product, since: endTime);
                        if ((Convert.ToInt64(currentUnixTime) - endTime) <= 0)
                        {
                            break;
                        }
                        for (int i = trades.Count - 1; i > 0; i--)
                        {
                            if (trades[i].time > endTime + 1)
                            {
                                string line =
                                trades[i].id + "," +
                                trades[i].time + "," +
                                trades[i].side + "," +
                                trades[i].price + "," +
                                trades[i].size;
                                sw.WriteLine(line);
                            }
                        }
                        endTime = (int)trades.First().time;
                        // Console.WriteLine("GMNI-{0} > {1}", product, endTime);
                        // Console.WriteLine(Convert.ToInt64(currentUnixTime) - endTime);
                        // Console.WriteLine();
                        // sw.WriteLine("=====");
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
