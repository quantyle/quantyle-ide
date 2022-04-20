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
    public class KrakenRequests
    {
        public Dictionary<string, string> Headers;
        public Dictionary<string, string> Parameters;
        private HttpClient client = new HttpClient();
        public string apiKey;

        public string uri;

        private readonly HMACSHA512 privateKey;

        public KrakenRequests()
        {
            Headers = new Dictionary<string, string>();
            Parameters = new Dictionary<string, string>();
            this.uri = "https://api.kraken.com";
            Key keys = readKeys(); // read keys from file
            this.apiKey = keys.key;
            this.privateKey = new HMACSHA512(Convert.FromBase64String(keys.secret));
        }

        public Key readKeys()
        {
            StreamReader r = new StreamReader("keys.json");
            string json = r.ReadToEnd();
            Dictionary<string, Key> keys = JsonConvert.DeserializeObject<Dictionary<string, Key>>(json);
            //Console.WriteLine(keys["GMNI"].passphrase);
            r.Close();
            return keys["KRKN"];
        }

        public static string ComputeSignature(string reqUrl, string nonce, HMACSHA512 privateKey, string urlEncodedArgs)
        {
            SHA256 sha256 = SHA256.Create();

            byte[] urlBytes = Encoding.UTF8.GetBytes(reqUrl);
            byte[] dataBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(nonce + urlEncodedArgs));

            var buffer = new byte[urlBytes.Length + dataBytes.Length];
            Buffer.BlockCopy(urlBytes, 0, buffer, 0, urlBytes.Length);
            Buffer.BlockCopy(dataBytes, 0, buffer, urlBytes.Length, dataBytes.Length);

            byte[] signature = privateKey.ComputeHash(buffer);
            return Convert.ToBase64String(signature);
        }


        public Task<HttpResponseMessage> Request(string path, HttpMethod method, Dictionary<string, string> parameters = null, bool auth = false)
        {
            string url = uri + path;

            Dictionary<string, string> headers = new Dictionary<string, string>();
            string postdata = "";
            // build auth headers
            if (auth)
            {
                //string timestamp = new DateTimeOffset(DateTime.Now.ToUniversalTime()).ToUnixTimeMilliseconds().ToString();
                string nonce = (DateTime.UtcNow.Ticks).ToString(CultureInfo.InvariantCulture);
                parameters.Add("nonce", nonce);

                // create pre-hash message
                postdata = parameters.Aggregate(
                    new StringBuilder(),
                    (sb, pair) => sb.AppendFormat("{0}={1}&", pair.Key, pair.Value),
                    (sb) => sb.ToString()).TrimEnd('&');

                string signature = ComputeSignature(path, nonce, privateKey, postdata);
                headers.Add("API-Key", this.apiKey);
                headers.Add("API-Sign", signature);
            }

            HttpRequestMessage request = new HttpRequestMessage(method, url);

            request.Content = new StringContent(postdata, Encoding.UTF8, "application/x-www-form-urlencoded");
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> p in headers)
                    request.Headers.Add(p.Key, p.Value);
            }

            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));

            return client.SendAsync(request);
        }
    }

    public class KrakenAssetPair
    {
        public string altname { get; set; }
        public string wsname { get; set; }
        [JsonProperty("base")]
        public string baseAsset { get; set; }
        [JsonProperty("quote")]
        public string quoteAsset { get; set; }

    }


    public class Kraken
    {
        public string uri = "https://api.kraken.com";

        public List<string[]> getProducts()
        {
            string path = "/0/public/AssetPairs";
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("info", "fees");

            KrakenRequests kraken = new KrakenRequests();
            var result = kraken.Request(path, HttpMethod.Post, parameters: param, auth: false).Result;

            // deserialize JSON to object
            string jsonContent = result.Content.ReadAsStringAsync().Result;

            // deserialize response
            // KrakenAssetInfo response = System.Text.Json.JsonSerializer.Deserialize<KrakenAssetInfo>(jsonContent);

            // var error = response.error.Count;
            // if (error > 0) throw new Exception("Kraken Error");
            // return response;

            //Console.WriteLine(jsonContent);
            var response = Newtonsoft.Json.Linq.JObject.Parse(jsonContent);
            List<string[]> products = new List<string[]>();
            //Console.WriteLine(response);
            // Console.WriteLine(response["result"].ToString());
            var pairs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, KrakenAssetPair>>(response["result"].ToString());
            foreach (KeyValuePair<string, KrakenAssetPair> kv in pairs)
            {
                products.Add(new string[]
                {
                    kv.Key,
                    response["result"][kv.Key]["base"].ToString(),
                    response["result"][kv.Key]["quote"].ToString(),
                });
            }
            return products;
        }
        public Dictionary<string, KrakenAssetInfo> getTradableAssetPairs()
        {
            string path = "/0/public/AssetPairs";
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("info", "fees");

            KrakenRequests kraken = new KrakenRequests();
            var result = kraken.Request(path, HttpMethod.Post, parameters: param, auth: false).Result;

            // deserialize JSON to object
            string jsonContent = result.Content.ReadAsStringAsync().Result;

            // deserialize response
            // KrakenAssetInfo response = System.Text.Json.JsonSerializer.Deserialize<KrakenAssetInfo>(jsonContent);

            // var error = response.error.Count;
            // if (error > 0) throw new Exception("Kraken Error");
            // return response;

            //Console.WriteLine(jsonContent);
            var response = Newtonsoft.Json.Linq.JObject.Parse(jsonContent);
            //Console.WriteLine(response);

            return response["result"].ToObject<Dictionary<string, KrakenAssetInfo>>();
        }

        public Dictionary<string, string> getAltNames()
        {
            Dictionary<string, KrakenAssetInfo> assetInfo = getTradableAssetPairs();
            Dictionary<string, string> altNames = new Dictionary<string, string>();

            foreach (KeyValuePair<string, KrakenAssetInfo> kv in assetInfo)
            {
                Console.WriteLine("{0}... {1}", kv.Key, assetInfo[kv.Key].altname);
                altNames.Add(assetInfo[kv.Key].altname, kv.Key);
            }

            return altNames;
        }

        public KrakenAssetInfo getAssetInfo()
        {
            string path = "/0/public/Assets";
            Dictionary<string, string> param = new Dictionary<string, string>();

            KrakenRequests kraken = new KrakenRequests();
            var result = kraken.Request(path, HttpMethod.Post, parameters: param, auth: false).Result;

            // deserialize JSON to object
            string jsonContent = result.Content.ReadAsStringAsync().Result;

            // deserialize response
            KrakenAssetInfo response = System.Text.Json.JsonSerializer.Deserialize<KrakenAssetInfo>(jsonContent);

            var error = response.error.Count;
            if (error > 0) throw new Exception("Kraken Error");
            return response;
            // foreach (KeyValuePair<string, Dictionary<string, object>> currency in response.result)
            // {
            //     Console.WriteLine("================ {0} ===============", currency.Key);
            //     foreach (KeyValuePair<string, object> info in currency.Value)
            //     {
            //         Console.WriteLine("{0}: {1}", info.Key, info.Value);
            //     }

            // }
        }


        public List<Ticker> getTrades(string productId, decimal since = 0)
        {
            string path = String.Format("{0}/0/public/Trades", uri);
            Dictionary<string, string> param = new Dictionary<string, string>();


            string baseCurrency = productId.Split("-")[0];
            string quoteCurrency = productId.Split("-")[1];
            if (baseCurrency == "BTC")
            {
                baseCurrency = "XBT";
            }

            if (since > 0)
            {

                param.Add("since", since.ToString());
                // Console.WriteLine(start.ToString());
            }
            // ETH-USD = XETHZUSD
            // XBT-USD = XXBTZUSD
            // this works:
            //string formattedProductId = "XBT/USD";

            string pair = baseCurrency + quoteCurrency;

            param.Add("pair", pair);
            // VALIDATE THAT LINE ABOVE WORKS

            string formattedProductId = "";
            // account for stupid formatting
            if (baseCurrency == "XBT" || baseCurrency == "ETH")
            {
                formattedProductId += "X" + baseCurrency + "Z" + quoteCurrency;
            }
            else
            {
                formattedProductId += baseCurrency + quoteCurrency;
                // Console.WriteLine(formattedProductId);
            }



            // HTTP GET
            var result = Requests.Get(path, parameters: param).Result;
            if (!result.IsSuccessStatusCode) throw new Exception("Bad response");
            // deserialize JSON to object
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            // Console.WriteLine(jsonContent);

            var array = Newtonsoft.Json.Linq.JObject.Parse(jsonContent);
            List<Ticker> trades = new List<Ticker>();

            foreach (var item in array["result"][formattedProductId])
            {
                trades.Add(new Ticker
                {
                    id = Convert.ToDecimal(item[2]),
                    time = Convert.ToDecimal(item[2]),
                    price = Convert.ToDecimal(item[0]),
                    size = Convert.ToDecimal(item[1]),
                    side = item[3].ToString() == "b" ? "buy" : "sell",
                });
            }
            return trades;
        }


        public List<List<decimal>> getChartList(string productId, decimal since = 0, decimal interval = 0)
        {
            string path = String.Format("{0}/0/public/OHLC", uri);
            Dictionary<string, string> param = new Dictionary<string, string>();


            string baseCurrency = productId.Split("-")[0];
            string quoteCurrency = productId.Split("-")[1];
            if (baseCurrency == "BTC")
            {
                baseCurrency = "XBT";
            }

            if (since > 0)
            {

                param.Add("since", since.ToString());
                // Console.WriteLine(start.ToString());
            }

            if (interval > 0)
            {
                param.Add("interval", interval.ToString());
            }

            // this works:
            //string formattedProductId = "XBT/USD";

            string formattedProductId = baseCurrency + quoteCurrency;

            param.Add("pair", formattedProductId);
            // VALIDATE THAT LINE ABOVE WORKS




            // HTTP GET
            var result = Requests.Get(path, parameters: param).Result;
            if (!result.IsSuccessStatusCode) throw new Exception("Bad response");
            // deserialize JSON to object
            string jsonContent = result.Content.ReadAsStringAsync().Result;

            // it aint pretty but it's fast
            string newcontent = "{\"data\": [[" + jsonContent.Replace("\"", "").Split("[[")[1].Split("]]")[0] + "]]}";
            ChartResponse chart = Utf8Json.JsonSerializer.Deserialize<ChartResponse>(newcontent);
            List<List<decimal>> chartList = new List<List<decimal>>();
            // row example: 
            // [time, open, high, low, close, vwap, volume, count]

            foreach (var c in chart.data)
            {
                chartList.Add(
                    new List<decimal>
                    {
                        Convert.ToDecimal(c[0]),
                        c[1],
                        c[2],
                        c[3],
                        c[4],
                        c[6],
                    }
                );
            }

            return chartList;
        }

        public void listOpenOrders()
        {
            // {"error":[],"result":{"ZUSD":"199.6800","XETH":"0.0496498400"}}

            string path = "/0/private/OpenOrders";
            Dictionary<string, string> param = new Dictionary<string, string>();

            KrakenRequests kraken = new KrakenRequests();
            var result = kraken.Request(path, HttpMethod.Post, parameters: param, auth: true).Result;
            if (!result.IsSuccessStatusCode) throw new Exception("Kraken - Bad response");

            // get result
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            // Console.WriteLine(jsonContent);

            var response = Newtonsoft.Json.Linq.JObject.Parse(jsonContent);
            var openOrders = response["result"]["open"];
            foreach (var obj in openOrders)
            {
                // var krakenOrder = JsonConvert.DeserializeObject<Dictionary<string, KrakenOpenOrder>>(obj.ToString());
                // KrakenOpenOrder openOrder = new KrakenOpenOrder{

                // }
                Console.WriteLine(">{0}", obj.ToString());
            }
            // JsonConvert.DeserializeObject<List<CoinbaseOpenOrder>>(jsonContent);




            // Dictionary<string, Balance> balances = new Dictionary<string, Balance>();

            // // deserialize response
            // KrakenBalances response = Utf8Json.JsonSerializer.Deserialize<KrakenBalances>(jsonContent);

            // // check for error
            // var error = response.error.Count;
            // if (error > 0) throw new Exception("Kraken Error");

            // foreach (KeyValuePair<string, string> kv in response.result)
            // {
            //     string currency = kv.Key.ToString().Remove(0, 1);  // <================================= this could be problematic !!!!
            //     balances.Add(currency, new Balance
            //     {
            //         exchange = "KRKN",
            //         currency = currency,
            //         altName = kv.Key.ToString(),
            //         amount = decimal.Parse(kv.Value),  // we only have a single value, so assume amount = available 
            //         available = decimal.Parse(kv.Value)

            //     });
            // }
            // return balances;
        }



        public Dictionary<string, Balance> listBalances()
        {
            // {"error":[],"result":{"ZUSD":"199.6800","XETH":"0.0496498400"}}

            string path = "/0/private/Balance";
            Dictionary<string, string> param = new Dictionary<string, string>();

            KrakenRequests kraken = new KrakenRequests();
            var result = kraken.Request(path, HttpMethod.Post, parameters: param, auth: true).Result;
            if (!result.IsSuccessStatusCode) throw new Exception("Kraken - Bad response");

            // get result
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            // Console.WriteLine(jsonContent);

            Dictionary<string, Balance> balances = new Dictionary<string, Balance>();

            // deserialize response
            KrakenBalances response = Utf8Json.JsonSerializer.Deserialize<KrakenBalances>(jsonContent);

            // check for error
            var error = response.error.Count;
            if (error > 0) throw new Exception("Kraken Error");

            foreach (KeyValuePair<string, string> kv in response.result)
            {
                string currency = kv.Key.ToString().Remove(0, 1);  // <================================= this could be problematic !!!!
                balances.Add(currency, new Balance
                {
                    exchange = "KRKN",
                    currency = currency,
                    altName = kv.Key.ToString(),
                    amount = decimal.Parse(kv.Value),  // we only have a single value, so assume amount = available 
                    available = decimal.Parse(kv.Value)

                });
            }
            return balances;
        }


        public Dictionary<string, Balance> getBalances()
        {
            // {"error":[],"result":{"ZUSD":"199.6800","XETH":"0.0496498400"}}

            string path = "/0/private/Balance";
            Dictionary<string, string> param = new Dictionary<string, string>();

            KrakenRequests kraken = new KrakenRequests();
            var result = kraken.Request(path, HttpMethod.Post, parameters: param, auth: true).Result;
            if (!result.IsSuccessStatusCode) throw new Exception("Kraken - Bad response");

            // get result
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            // Console.WriteLine(jsonContent);

            Dictionary<string, Balance> balances = new Dictionary<string, Balance>();

            // deserialize response
            KrakenBalances response = Utf8Json.JsonSerializer.Deserialize<KrakenBalances>(jsonContent);

            // check for error
            var error = response.error.Count;
            if (error > 0) throw new Exception("Kraken Error");

            foreach (KeyValuePair<string, string> kv in response.result)
            {
                string currency = kv.Key.ToString().Remove(0, 1);  // <================================= this could be problematic !!!!
                balances.Add(currency, new Balance
                {
                    exchange = "KRKN",
                    currency = currency,
                    altName = kv.Key.ToString(),
                    amount = decimal.Parse(kv.Value),  // we only have a single value, so assume amount = available 
                    available = decimal.Parse(kv.Value)

                });
            }
            return balances;
        }


        public string getWebSocketsToken(string baseCurrency = "USD")
        {
            string path = "/0/private/GetWebSocketsToken";
            Dictionary<string, string> param = new Dictionary<string, string>();

            KrakenRequests kraken = new KrakenRequests();
            var result = kraken.Request(path, HttpMethod.Post, parameters: param, auth: true).Result;
            if (!result.IsSuccessStatusCode) throw new Exception("Kraken - Bad response");

            // get result
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            // Console.WriteLine("KRKN Websockets Token: {0}", jsonContent);
            var response = Newtonsoft.Json.Linq.JObject.Parse(jsonContent);
            string token = response["result"]["token"].ToString();
            return token;
        }


        public void getTradeBalances()
        {
            string path = "/0/private/TradeBalance";
            Dictionary<string, string> param = new Dictionary<string, string>();

            KrakenRequests kraken = new KrakenRequests();
            var result = kraken.Request(path, HttpMethod.Post, parameters: param, auth: true).Result;
            if (!result.IsSuccessStatusCode) throw new Exception("Kraken - Bad response");

            // deserialize JSON to object
            string jsonContent = result.Content.ReadAsStringAsync().Result;

            //KrakenBalances response = Utf8Json.JsonSerializer.Deserialize<KrakenBalances>(jsonContent);

            // Console.WriteLine(jsonContent);
        }

        // public T[] Reverse<T>(T[] array)
        // {
        //     var result = new T[array.Length];
        //     int j = 0;
        //     for (int i = array.Length - 1; i >= 0; i--)
        //     {
        //         result[j] = array[i];
        //         j++;
        //     }
        //     return result;
        // }

        public List<decimal> getFees(string pair = "XBTUSD")
        {
            string path = "/0/private/TradeVolume";
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("pair", pair);
            param.Add("fee-info", "true");

            KrakenRequests kraken = new KrakenRequests();
            var result = kraken.Request(path, HttpMethod.Post, parameters: param, auth: true).Result;
            if (!result.IsSuccessStatusCode) throw new Exception("Kraken - Bad response");

            // deserialize JSON to object
            string jsonContent = result.Content.ReadAsStringAsync().Result;

            //Console.WriteLine("Kraken.getTradeVolume: {0}", jsonContent);

            // use JObject to parse complex objects
            var response = Newtonsoft.Json.Linq.JObject.Parse(jsonContent);
            Dictionary<string, Dictionary<string, string>> makerFees = response["result"]["fees_maker"].ToObject<Dictionary<string, Dictionary<string, string>>>();
            Dictionary<string, Dictionary<string, string>> takerFees = response["result"]["fees"].ToObject<Dictionary<string, Dictionary<string, string>>>();


            string defaultKey = takerFees.Keys.First();
            //Console.WriteLine(defaultKey);
            string makerFeeStr = takerFees[defaultKey]["fee"].Replace(".", ".00");
            string takerFeeStr = makerFees[defaultKey]["fee"].Replace(".", ".00");

            decimal makerFee = decimal.Parse(makerFeeStr);
            decimal takerFee = decimal.Parse(takerFeeStr);

            //string volume = response["result"]["volume"].ToString();


            // G29 -> remove trailing zeros up to 29 sig figs (decimal maximum)
            return new List<decimal> { decimal.Parse(makerFee.ToString("G29")), decimal.Parse(takerFee.ToString("G29")) };
        }



        public void loadTrades(string product, decimal currentUnixTime, decimal endTime, int delay = 1000)
        {

            string path = "./media/data/KRKN-" + product + "-trades.csv"; // reversed 
            bool fileFound = File.Exists(path);
            Encoding encoding = Encoding.UTF8;

            // if the file exists, update it
            if (fileFound)
            {
                // remove the last line as it could be corrupt

                List<string> lastLines = Files.ReadFileReverse2(path, 10);


                // Files.RemoveLastLine(path);
                // string lastLine = Files.ReadLastLine(path);
                endTime = (int)decimal.Parse(lastLines[1].Split(",")[1]);
                
                int delta = 0;
                Console.WriteLine(" >>>>>>>>{0}", endTime);

                // remove last line(s) that include the same timestamp (ignoring the decimal)
                for (var i = 0; i < lastLines.Count; i++)
                {
                    Console.WriteLine(">>>>>>>>{0}", lastLines[i]);

                    var lastLine = lastLines[i];

                    if (lastLine.Split(",").Length == 5)
                    {

                        if ((int)decimal.Parse(lastLine.Split(",")[1]) != endTime)
                        {
                            endTime = (int)decimal.Parse(lastLine.Split(",")[1]);
                            break;
                        }
                    }
                    delta += lastLine.Length + 1;
                }


                // if the file exists we append the text, otherwise we create the file
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    stream.Seek(-1 * (delta - 1), SeekOrigin.End);
                    decimal prevLastTime = -1;

                    while (true)
                    {
                        Console.WriteLine("updating file for KRKN-{0}... {1}", product, (Convert.ToInt64(currentUnixTime) - endTime));
                        var trades = getTrades(product, since: endTime);

                        // break if we see the same time again (the API gives no new updates)
                        if (endTime == prevLastTime)
                        {
                            break;
                        }
                        else
                        {
                            prevLastTime = endTime;
                        }

                        for (int i = 0; i < trades.Count; i++)
                        {
                            if (trades[i].time > endTime + 1)
                            {
                                string line =
                                trades[i].id + "," +
                                trades[i].time + "," +
                                trades[i].side + "," +
                                trades[i].price + "," +
                                trades[i].size + "\n";
                                stream.Write(encoding.GetBytes(line), 0, encoding.GetByteCount(line));
                                // Console.WriteLine(line);
                                // sw.WriteLine(line);
                            }
                            // Console.WriteLine("writing file for KRKN-{0}", product);
                        }
                        endTime = (int)trades.Last().time;
                        // Console.WriteLine("KRKN-{0} > {1}", product, endTime);
                        // Console.WriteLine(Convert.ToInt64(currentUnixTime) - endTime);
                        // Console.WriteLine();
                        // sw.WriteLine("=====");
                        Thread.Sleep(delay);
                    }
                }
            }
            else
            {
                // if the file is not found, 
                Console.WriteLine("creating file for KRKN-{0}", product);

                // if the file exists we append the text, otherwise we create the file
                using (StreamWriter sw = File.CreateText(path))
                {

                    while (true)
                    {
                        // trades = binanceCli.getTrades("BTC-USD", fromId: lastId + 1).data;
                        var trades = getTrades(product, since: endTime);
                        if ((Convert.ToInt64(currentUnixTime) - endTime) <= 0)
                        {
                            break;
                        }
                        for (int i = 0; i < trades.Count; i++)
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
                        endTime = (int)trades.Last().time;
                        // Console.WriteLine("KRKN-{0} > {1}", product, endTime);
                        // Console.WriteLine(Convert.ToInt64(currentUnixTime) - endTime);
                        // Console.WriteLine();
                        // sw.WriteLine("=====");
                        Thread.Sleep(delay);
                    }
                }
            }

        }




        public void createChartFile(string productId, decimal start, decimal end, int delay)
        {
            // @TODO: move the method to a helper class for modifiying static files
            string path = "./media/data/KRKN-" + productId + "-candles.csv";
            string pathRev = "./media/data/KRKN-" + productId + "-candles-rev.csv";
            decimal timestamp = end;
            Console.WriteLine("end time: {0}", timestamp);

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
                    timestamp = decimal.Parse(lastLine.Split(",")[0]) - 60;
                }


                // load the file in reverse order first
                using (StreamWriter sw = reverseFound ? File.AppendText(pathRev) : File.CreateText(pathRev))
                {
                    while (true)
                    {
                        var chart = getChartList(productId, since: timestamp, interval: 1);
                        for (int i = 0; i < chart.Count; i++)
                        {
                            string line =
                            chart[i][0] + "," +
                            chart[i][1] + "," +
                            chart[i][2] + "," +
                            chart[i][3] + "," +
                            chart[i][4] + "," +
                            chart[i][5];
                            sw.WriteLine(line);
                        }
                        sw.WriteLine("======");
                        Console.WriteLine("----");

                        timestamp = chart.First()[0] - 60;
                        Console.WriteLine(">>>{0}", timestamp);
                        // if (timestamp >= start)
                        // {
                        //     break;
                        // }

                        Thread.Sleep(delay);
                    }
                }

                // // write the final file
                // using (StreamWriter sw = File.CreateText(path))
                // {
                //     Files.ReverseFile(pathRev, path);
                //     File.Delete(pathRev);
                //     Console.WriteLine("{0} created.", path);
                // }
            }
            else
            {
                Console.WriteLine("{0} found. skipping creation", path);
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
