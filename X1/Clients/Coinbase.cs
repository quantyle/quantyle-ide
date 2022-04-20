using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Globalization;
using System.Security.Cryptography;
using System.Threading;
using System.IO;
using Newtonsoft.Json;
// using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;

namespace X1.Clients
{
    public class CoinbaseRequests
    {
        public string Data;
        public Dictionary<string, string> Headers;
        public Dictionary<string, string> Parameters;
        private HttpClient client = new HttpClient();
        public string apiKey;
        public string unsignedSignature;
        public string passphrase;
        public string Url;

        public CoinbaseRequests(bool auth = false)
        {
            Headers = new Dictionary<string, string>();
            Parameters = new Dictionary<string, string>();
            this.Url = "https://api.pro.coinbase.com";
            this.Data = "";
            if (auth)
            {
                Key key = readKeys(); // read keys from file
                this.apiKey = key.key;
                this.unsignedSignature = key.secret;
                this.passphrase = key.passphrase;
            }
        }

        private static string HashString(string str, byte[] secret)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            using (var hmaccsha = new HMACSHA256(secret))
            {
                return Convert.ToBase64String(hmaccsha.ComputeHash(bytes));
            }
        }

        public static string ComputeSignature(
            HttpMethod httpMethod,
            string secret,
            double timestamp,
            string requestUri,
            string contentBody = "")
        {
            var convertedString = Convert.FromBase64String(secret);
            var prehash = timestamp.ToString("F0", CultureInfo.InvariantCulture) + httpMethod.ToString().ToUpper() + requestUri + contentBody;
            return HashString(prehash, convertedString);
        }
        public static Key readKeys()
        {
            StreamReader r = new StreamReader("keys.json");
            string json = r.ReadToEnd();
            Dictionary<string, Key> keys = JsonConvert.DeserializeObject<Dictionary<string, Key>>(json);
            //// Console.WriteLine(keys["GDAX"].passphrase);
            r.Close();
            return keys["GDAX"];
        }

        public Task<HttpResponseMessage> Get(string path, string data = "", Dictionary<string, string> parameters = null, bool auth = false)
        {
            string fullPath = this.Url + path;

            // add parameters to the request query
            if (parameters != null)
            {
                fullPath += "?" + parameters.Aggregate(
                 new StringBuilder(),
                 (sb, pair) => sb.AppendFormat("{0}={1}&", pair.Key, pair.Value),
                 (sb) => sb.ToString());
                // fullPath = fullPath.TrimEnd('&');
                // // Console.WriteLine("GET: " + fullPath);
            }

            // build authnetication headers 
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("User-Agent", "CoinbaseProClient");

            if (auth)
            {
                var timestamp = DateTime.UtcNow.ToTimestamp();
                var signedSignature = ComputeSignature(HttpMethod.Get, this.unsignedSignature, timestamp, path);
                headers.Add("CB-ACCESS-KEY", this.apiKey);
                headers.Add("CB-ACCESS-TIMESTAMP", timestamp.ToString("F0", CultureInfo.InvariantCulture));
                headers.Add("CB-ACCESS-SIGN", signedSignature);
                headers.Add("CB-ACCESS-PASSPHRASE", this.passphrase);
            }

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, fullPath);
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

        public Task<HttpResponseMessage> Post(string url, string data = "", Dictionary<string, string> headers = null, Dictionary<string, string> parameters = null)
        {
            if (parameters != null)
            {
                url += parameters.Aggregate(
                 new StringBuilder(),
                 (sb, pair) => sb.AppendFormat("{0}={1}&", pair.Key, pair.Value),
                 (sb) => sb.ToString());
            }

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            if (data != String.Empty)
                request.Content = new StringContent(data, Encoding.UTF8);
            foreach (KeyValuePair<string, string> p in headers)
                request.Headers.Add(p.Key, p.Value);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));
            return client.SendAsync(request);
        }

        // private Uri GenerateUri()
        // {
        //     return new Uri(this.Url +
        //         this.Parameters.Aggregate(
        //             new StringBuilder(),
        //             (sb, pair) => sb.AppendFormat("{0}={1}&", pair.Key, pair.Value),
        //             (sb) => sb.ToString()));
        // }

        // public byte[] SubscriptionMessage()
        // {
        //     // build subscription message
        //     long timestamp = DateTime.UtcNow.ToTimestamp();
        //     string signedSignature = ComputeSignature(HttpMethod.Get, this.unsignedSignature, timestamp, "/users/self/verify");
        //     string json = "{\"type\": \"subscribe\", \"product_ids\": [\"" + "BTC-USD" + "\"], \"channels\": [\"level2\", \"ticker\"], \"signature\": \"" + signedSignature + "\", \"key\": \"" + this.apiKey + "\", \"passphrase\": \"" + this.passphrase + "\", \"timestamp\": \"" + timestamp + "\"}";
        //     // Console.WriteLine(json);
        //     return Encoding.UTF8.GetBytes(json);
        // }

        // public string ComputeSignature(
        //     HttpMethod httpMethod,
        //     string secret,
        //     double timestamp,
        //     string requestUri,
        //     string contentBody = "application/json")
        // {
        //     var convertedString = Convert.FromBase64String(secret);
        //     var prehash = timestamp.ToString("F0", CultureInfo.InvariantCulture) + httpMethod.ToString().ToUpper() + requestUri + contentBody;
        //     return HashString(prehash, convertedString);
        // }

    }

    // {
    //     "id": "BTC-USD",
    //     "display_name": "BTC/USD",
    //     "base_currency": "BTC",
    //     "quote_currency": "USD",
    //     "base_increment": "0.00000001",
    //     "quote_increment": "0.01000000",
    //     "base_min_size": "0.00100000",
    //     "base_max_size": "280.00000000",
    //     "min_market_funds": "5",
    //     "max_market_funds": "1000000",
    //     "status": "online",
    //     "status_message": "",
    //     "cancel_only": false,
    //     "limit_only": false,
    //     "post_only": false,
    //     "trading_disabled": false
    // },



    // {
    //     "id":"OMG-BTC",
    //     "base_currency":"OMG",
    //     "quote_currency":"BTC",
    //     "base_min_size":"0.1",
    //     "base_max_size":"14000",

    //     "quote_increment":"0.00000001",
    //     "base_increment":"0.1",
    //     "display_name":"OMG/BTC",
    //     "min_market_funds":"0.000016",
    //     "max_market_funds":"3",

    //     "margin_enabled":false,
    //     "fx_stablecoin":false,
    //     "max_slippage_percentage":"0.03000000",
    //     "post_only":false,
    //     "limit_only":false,
    //     "cancel_only":false,
    //     "trading_disabled":false,
    //     "status":"online",
    //     "status_message":"",
    //     "auction_mode":false
    // }

    public class CoinbaseProduct
    {
        public string id { get; set; }
        public string base_currency { get; set; }
        public string quote_currency { get; set; }
        public string base_min_size { get; set; }
        public string base_max_size { get; set; }

        public string quote_increment { get; set; }
        public string base_increment { get; set; }
        public string display_name { get; set; }
        // public string min_market_funds { get; set; }
        // public string max_market_funds { get; set; }

        // public bool margin_enabled { get; set; }
        // public bool fx_stablecoin { get; set; }
        // public string max_slippage_percentage { get; set; }
        // public string status { get; set; }
        // public string status_message { get; set; }
        // public bool post_only { get; set; }
        // public bool limit_only { get; set; }
        // public bool cancel_only { get; set; }
        // public bool trading_disabled { get; set; }
        // public bool auction_mode { get; set; }
    }

    public class CoinbaseProducts
    {
        public IList<CoinbaseProduct> data { get; set; }
    }

    public class Coinbase
    {

        // def get_products(self):
        //     # HTTP GET all products

        //     response = self.session.request(
        //         method='get',
        //         url=self.api_url + '/products',
        //         timeout=self.timeout,
        //     ).json()
        //     return response

        public List<string[]> getProducts()
        {

            string path = "/products";

            CoinbaseRequests cb = new CoinbaseRequests();
            // HTTP GET
            var result = cb.Get(path, auth: true).Result;
            //if (!result.IsSuccessStatusCode) throw new Exception("Bad response");

            // deserialize JSON to object
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            // Console.WriteLine(jsonContent);

            // var data = System.Text.Json.JsonSerializer.Deserialize<CoinbaseProducts>("{\"data\": " + jsonContent + "}");

            var data = Newtonsoft.Json.Linq.JObject.Parse("{\"data\": " + jsonContent + "}");
            List<string[]> products = new List<string[]>();
            foreach (var product in data["data"])
            {
                products.Add(new string[]{
                    product["id"].ToString(),
                    product["base_currency"].ToString(),
                    product["quote_currency"].ToString(),
                });
                // // Console.WriteLine(product["id"].ToString());
                //     // Console.WriteLine(product["base_currency"].ToString());
                //     // Console.WriteLine(product["quote_currency"].ToString());
            }
            return products;
        }

        public Dictionary<string, Balance> getBalances()
        {
            string path = "/accounts";
            CoinbaseRequests cb = new CoinbaseRequests();
            var result = cb.Get(path, auth: true).Result;

            Dictionary<string, Balance> balances = new Dictionary<string, Balance>();

            // deserialize JSON to object
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            // // Console.WriteLine(jsonContent);
            var response = Newtonsoft.Json.Linq.JObject.Parse("{\"data\": " + jsonContent + "}");
            foreach (var item in response["data"])
            {
                string currency = item["currency"].ToString();
                decimal amount = decimal.Parse(item["balance"].ToString());
                decimal available = decimal.Parse(item["available"].ToString());

                // only include currencies with a balance or amount greater than 0
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

        // public decimal getBalance(string baseCurrency = "USD")
        // {
        //     CoinbaseAccounts coinbaseAccounts = getAccounts();

        //     // search backwards since we know USD is towards the end of the list
        //     for (int i = coinbaseAccounts.data.Count - 1; i >= 0; i--)
        //     {
        //         Dictionary<string, object> item = coinbaseAccounts.data[i];
        //         // check if we found "USD"
        //         if (item["currency"].ToString() == baseCurrency)
        //         {
        //             return decimal.Parse(item["balance"].ToString());
        //         }
        //     }
        //     throw new Exception("missing Coinbase balance!");
        // }

        // public void updateChartFile(string productId)
        // {
        //     // @TODO: move the method to a helper class for modifiying static files
        //     string fname = "charts/GDAX/" + productId + ".csv";
        //     var lastLine = File.ReadLines(fname).Last();
        //     var lastLineList = lastLine.Split(",");
        //     long lastCandleTime = Int64.Parse(lastLineList[0]);
        //     long delta_t = 18000;

        //     //1636625280,65276.45,65314.97,65282.28,65314.97,5.40075636
        //     while (true)
        //     {
        //         ChartResponse chart = getChartObject(productId, lastCandleTime, (lastCandleTime + delta_t));
        //         if (chart.data.Count > 0)
        //         {
        //             using (StreamWriter sw = File.AppendText(fname))
        //             {
        //                 for (var i = chart.data.Count - 1; i >= 0; i--)
        //                 {
        //                     var c = chart.data[i];
        //                     string line = c[0] + "," + c[1] + "," + c[2] + "," + c[3] + "," + c[4] + "," + c[5];
        //                     //// Console.WriteLine(line);
        //                     sw.WriteLine("writing: {0}", line);
        //                 }
        //             }
        //         }
        //         else
        //         {
        //             // Console.WriteLine("{0} candles up to date", productId);
        //             break;
        //         }
        //         lastCandleTime += delta_t;
        //         // sleep so we dont anger the API
        //         Thread.Sleep(500);
        //     }
        // }

        public List<decimal> getFees()
        {
            string path = "/fees";

            CoinbaseRequests cb = new CoinbaseRequests();
            // HTTP GET
            var result = cb.Get(path, auth: true).Result;
            //if (!result.IsSuccessStatusCode) throw new Exception("Bad response");

            // deserialize JSON to object
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            Dictionary<string, string> volumeAndFees = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
            //return new List<string> { volumeAndFees["maker_fee_rate"], volumeAndFees["taker_fee_rate"], volumeAndFees["usd_volume"] };
            return new List<decimal> { decimal.Parse(volumeAndFees["maker_fee_rate"]), decimal.Parse(volumeAndFees["taker_fee_rate"]) };

        }

        public CoinbaseProfiles getProfiles()
        {
            string path = "/profiles";

            CoinbaseRequests cb = new CoinbaseRequests();
            // HTTP GET
            var result = cb.Get(path, auth: true).Result;
            //if (!result.IsSuccessStatusCode) throw new Exception("Bad response");

            // deserialize JSON to object
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            return System.Text.Json.JsonSerializer.Deserialize<CoinbaseProfiles>("{\"data\": " + jsonContent + "}");
        }



        public CoinbaseAccounts getAccounts()
        {
            string path = "/accounts";
            CoinbaseRequests cb = new CoinbaseRequests();
            var result = cb.Get(path, auth: true).Result;
            //if (!result.IsSuccessStatusCode) throw new Exception("Bad response");

            // deserialize JSON to object
            string jsonContent = result.Content.ReadAsStringAsync().Result;

            // Console.WriteLine(jsonContent);
            return System.Text.Json.JsonSerializer.Deserialize<CoinbaseAccounts>("{\"data\": " + jsonContent + "}");
        }


        public List<CoinbaseOpenOrder> listOrders(string productId)
        {
            string path = String.Format("/orders?product_id={0}", productId);
            CoinbaseRequests cb = new CoinbaseRequests();
            // parameters
            var result = cb.Get(path, auth: true).Result;
            //if (!result.IsSuccessStatusCode) throw new Exception("Bad response");
            // deserialize JSON to object
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            // Console.WriteLine(jsonContent);
            return JsonConvert.DeserializeObject<List<CoinbaseOpenOrder>>(jsonContent);
        }

        public List<CoinbaseOpenOrder> listAllOrders()
        {
            string path = "/orders";
            CoinbaseRequests cb = new CoinbaseRequests();
            // parameters
            var result = cb.Get(path, auth: true).Result;
            // deserialize JSON to object
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            // Console.WriteLine(jsonContent);
            return JsonConvert.DeserializeObject<List<CoinbaseOpenOrder>>(jsonContent);
        }


        public List<Ticker> getTrades(string productId, decimal after = 0, decimal before = 0)
        {
            // The trade side indicates the maker order side. 
            // The maker order is the order that was open on the order book. 
            // buy side indicates a down-tick because the maker was a buy 
            // order and their order was removed. Conversely, sell side 
            // indicates an up-tick.

            // {
            //     "time":"2022-01-21T08:25:03.877711Z",
            //     "trade_id":266929077,
            //     "price":"39113.14000000",
            //     "size":"0.00132600",
            //     "side":"buy"
            // }
            string path = String.Format("/products/{0}/trades", productId);

            // parameters
            Dictionary<string, string> param = new Dictionary<string, string>();
            if (after > 0)
            {
                param.Add("after", after.ToString());
            }

            if (before > 0)
            {
                param.Add("before", before.ToString());
            }

            CoinbaseRequests cb = new CoinbaseRequests();
            // HTTP GET
            var result = cb.Get(path, parameters: param, auth: false).Result;
            // if (!result.IsSuccessStatusCode) throw new Exception("Bad response");

            // deserialize JSON to object
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            // Console.WriteLine(jsonContent);
            // Console.WriteLine();

            JObject array = JsonConvert.DeserializeObject<JObject>(
                "{\"data\": " + jsonContent + "}",
                new JsonSerializerSettings { DateParseHandling = DateParseHandling.None }
            );
            List<Ticker> trades = new List<Ticker>();
            try
            {
                foreach (var item in array["data"])
                {

                    // long timestamp = DateTime.Parse(item["time"].ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind).ToTimestamp();
                    decimal timestamp = DateTime.Parse(item["time"].ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind).ToTimestampMs();
                    // // Console.WriteLine(timestamp / 1000);
                    // // Console.WriteLine(DateTime.ParseExact(item["time"].ToString(), format, provider).ToTimestamp());
                    trades.Add(new Ticker
                    {
                        id = Convert.ToDecimal(item["trade_id"]),
                        time = (timestamp / 1000),
                        price = Convert.ToDecimal(item["price"]) / 1.000000000000000000000000000000000m,
                        size = Convert.ToDecimal(item["size"]) / 1.000000000000000000000000000000000m,
                        side = item["side"].ToString(),
                    });
                }
            }
            catch (Exception)
            {
                return trades;
            }
            return trades;
        }
        public ChartResponse getChartObject(string productId, decimal start = 0, decimal end = 0)
        {
            string granularity = "60";
            string path = String.Format("/products/{0}/candles", productId);

            // parameters
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("granularity", granularity);

            // start date param
            if (start > 0)
            {

                DateTimeOffset startDateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(start));
                string startDateTime = startDateTimeOffset.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
                param.Add("start", startDateTime);
                // Console.WriteLine(startDateTime);
            }

            // end date param
            if (end > 0)
            {
                // Console.WriteLine(Convert.ToInt64(end));
                DateTimeOffset endDateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(end));
                string endDateTime = endDateTimeOffset.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
                param.Add("end", endDateTime);
                // Console.WriteLine(endDateTime);
            }


            CoinbaseRequests cb = new CoinbaseRequests();
            // HTTP GET
            var result = cb.Get(path, parameters: param, auth: false).Result;
            if (!result.IsSuccessStatusCode) throw new Exception("Bad response");

            // deserialize JSON to object
            string jsonContent = result.Content.ReadAsStringAsync().Result;

            return System.Text.Json.JsonSerializer.Deserialize<ChartResponse>("{\"data\": " + jsonContent + "}");
        }

        public List<List<decimal>> getChartList(string productId, decimal start = 0, decimal end = 0)
        {
            ChartResponse chart = getChartObject(productId, start, end);
            List<List<decimal>> chartList = new List<List<decimal>>();

            for (int i = chart.data.Count - 1; i >= 0; i--)
            {
                var c = chart.data[i];
                chartList.Add(
                    new List<decimal>
                    {
                        Convert.ToDecimal(c[0]),
                        c[3],  // open
                        c[2],  // high
                        c[1],  // low
                        c[4],  // close
                        c[5],  // volume
                    }
                );
            }
            return chartList;
        }

        // get the chart using ticks file, instead of from the API
        public List<List<decimal>> getChartListV2(string productId, decimal start = 0, decimal end = 0)
        {
            // ChartResponse chart = getChartObject(productId, start, end);
            List<List<decimal>> chartList = new List<List<decimal>>();
            string candlespath = "./media/data/GDAX-" + productId + "-candles.csv";
            Encoding encoding = Encoding.ASCII;
            int charsize = encoding.GetByteCount("\n");
            byte[] buffer = encoding.GetBytes("\n");
            char[] chars;
            string rev = "";

            using (FileStream sr = new FileStream(candlespath, FileMode.Open))
            {
                long endpos = sr.Length;
                string line = "";
                for (long pos = charsize; pos < endpos + charsize; pos += charsize)
                {
                    sr.Seek(-pos, SeekOrigin.End);
                    sr.Read(buffer, 0, buffer.Length);
                    var b = encoding.GetString(buffer);

                    if (b == "\n")
                    {
                        chars = line.ToCharArray();
                        Array.Reverse(chars);
                        rev = new string(chars);

                        if (rev.Length > 0)
                        {
                            string[] array = rev.Split(",");
                            chartList.Insert(0,
                                new List<decimal>
                                {
                                    Convert.ToDecimal(array[0]),
                                    Convert.ToDecimal(array[1]),  // open
                                    Convert.ToDecimal(array[2]),  // high
                                    Convert.ToDecimal(array[3]),  // low
                                    Convert.ToDecimal(array[4]),  // close
                                    Convert.ToDecimal(array[5]),  // volume
                                }
                            );
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
                // Console.WriteLine(rev);
                string[] arr = rev.Split(",");
                chartList.Insert(0,
                    new List<decimal>
                    {
                        Convert.ToDecimal(arr[0]),
                        Convert.ToDecimal(arr[1]),  // open
                        Convert.ToDecimal(arr[2]),  // high
                        Convert.ToDecimal(arr[3]),  // low
                        Convert.ToDecimal(arr[4]),  // close
                        Convert.ToDecimal(arr[5]),  // volume
                    }
                );
            }

            return chartList;
        }
        // public string updateTrades(string product)
        // {
        //     string path = "./media/data/GDAX-" + product + "-trades.csv";
        //     string lastLine = Files.ReadLastLine(path);
        //     decimal lastId = Convert.ToDecimal(lastLine.Split(",")[0]);
        //     string line = "";

        //     // if the file exists we append the text, otherwise we create the file
        //     using (StreamWriter sw = File.AppendText(path))
        //     {
        //         var trades = getTrades(product);
        //         int limit = trades.Count;

        //         // write to file
        //         for (int i = trades.Count - 1; i > 0; i--)
        //         {
        //             decimal id = Convert.ToDecimal(trades[i].id);
        //             line =
        //             trades[i].id + "," +
        //             trades[i].time + "," +
        //             trades[i].side + "," +
        //             trades[i].price + "," +
        //             trades[i].size;

        //             if (id > lastId)
        //             {
        //                 sw.WriteLine(line);
        //                 // // Console.WriteLine("> {0}", line);
        //             }
        //         }
        //     }
        //     return line;
        // }


        public void loadTrades(string product, decimal currentUnixTime, decimal endTime, int delay = 600)
        {
            string path = "./media/data/GDAX-" + product + "-trades.csv";
            string pathRev = "./media/data/GDAX-" + product + "-trades-rev.csv";
            var trades = getTrades(product);
            decimal lastTime = 0;
            int limit = trades.Count;
            var lastId = trades.First().id - limit;
            bool fileFound = File.Exists(pathRev);
            bool outputFound = File.Exists(path);

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
                    Console.WriteLine("repairing file for GDAX-{0}", product);
                }
                else
                {
                    Console.WriteLine("creating file for GDAX-{0}", product);
                }

                // if the file exists we append the text, otherwise we create the file
                using (StreamWriter sw = File.CreateText(pathRev))
                {
                    while (true)
                    {
                        trades = getTrades(product, after: lastId + 1);
                        lastTime = Convert.ToDecimal(trades.Last().time);
                        if ((lastTime - endTime) <= 0)
                        {
                            break;
                        }

                        // write to file
                        for (int i = 0; i < trades.Count; i++)
                        {
                            string line =
                            trades[i].id + "," +
                            trades[i].time + "," +
                            trades[i].side + "," +
                            trades[i].price + "," +
                            trades[i].size;
                            sw.WriteLine(line);
                        }

                        lastId = Convert.ToDecimal(trades.First().id) - limit;
                        Thread.Sleep(delay);
                    }
                }

                Files.RemoveLastLine(pathRev);
                Files.ReverseFile(pathRev, path);
                File.Delete(pathRev);
            }
            else
            {
                // if the file is not found, 
                Console.WriteLine("updating file for GDAX-{0}", product);
                Files.RemoveLastLine(path);
                // reverse the file, and change the path
                // Console.WriteLine("removed last line");

                List<string> lastLines = Files.ReadFileReverse2(path, 4);


                // string lastLine = Files.ReadLastLine(path);
                lastId = (int)decimal.Parse(lastLines[1].Split(",")[0]) + limit;
                StringBuilder sb = new StringBuilder();
                Encoding encoding = Encoding.UTF8;

                // if the file exists we append the text, otherwise we create the file
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    // // Console.WriteLine("len >>>> {0}", lastLines.Count);

                    // go to end of the file, minus the last few bytes
                    stream.Seek(-1 * (lastLines[0].Length), SeekOrigin.End);
                    while (true)
                    {
                        trades = getTrades(product, after: lastId + 1);
                        lastTime = Convert.ToDecimal(trades.Last().time);

                        if ((Convert.ToDecimal(trades.First().id) - lastId) < 0)
                        {
                            // write to file
                            for (int i = trades.Count - 1; i > 0; i--)
                            {
                                if (Convert.ToDecimal(trades[i].id) > (lastId - limit))
                                {
                                    sb.Append(trades[i].id);
                                    sb.Append(",");
                                    sb.Append(trades[i].time);
                                    sb.Append(",");
                                    sb.Append(trades[i].side);
                                    sb.Append(",");
                                    sb.Append(trades[i].price);
                                    sb.Append(",");
                                    sb.Append(trades[i].size);
                                    sb.AppendLine();
                                }
                            }
                            break;
                        }

                        // write to file
                        for (int i = trades.Count - 1; i > 0; i--)
                        {
                            sb.Append(trades[i].id);
                            sb.Append(",");
                            sb.Append(trades[i].time);
                            sb.Append(",");
                            sb.Append(trades[i].side);
                            sb.Append(",");
                            sb.Append(trades[i].price);
                            sb.Append(",");
                            sb.Append(trades[i].size);
                            sb.AppendLine();
                        }

                        decimal timeDiff = currentUnixTime - lastTime;
                        Console.WriteLine("GDAX-{0}: {1}", product, timeDiff);

                        stream.Write(encoding.GetBytes(sb.ToString()), 0, encoding.GetByteCount(sb.ToString()));
                        sb.Clear();

                        lastId = Convert.ToDecimal(trades.First().id) + limit - 1;
                        // // Console.WriteLine();
                        Thread.Sleep(delay);
                    }
                }
            }

            // // -------------------------------- load candles files --------------------------------
            // // ------------------ ERROR: Uncomment after removing last line from candles to fix unfinished candles ------------------------
            Files.LoadCandlesFromTicks("GDAX", product);
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


