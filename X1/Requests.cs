using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Globalization;
using System.Security.Cryptography;
using System.IO;
using Newtonsoft.Json;

namespace X1
{

	public class Time
    {
        public static long Timestamp()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }

        public static long TimestampMs()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
        }
    }

    public static class Extensions
    {
        public static T Json<T>(this HttpResponseMessage message)
        {
            var stream = message.Content.ReadAsStreamAsync();
            return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(stream.Result);
        }

		public static T Json<T>(this string message)
        {
            var stream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(message));

            return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(stream);
        }

        public static long ToTimestamp(this DateTime dt)
        {
            return (long)(dt - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }


        public static long ToTimestampMs(this DateTime dt)
        {
            return (long)(dt - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
        }
    }


    public class Requests
    {
		// public string Url { get; set; }
		// public string ContentType { get; set; }
		// public string Data { get; set; }
		// public Dictionary<string, string> Headers { get; set; }
		// public Dictionary<string, string> Parameters { get; set; }
        private static HttpClient client = new HttpClient();

        public static string ComputeSignature(
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

        private static string HashString(string str, byte[] secret)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            using (var hmaccsha = new HMACSHA256(secret))
            {
                return Convert.ToBase64String(hmaccsha.ComputeHash(bytes));
            }
        }
        public static Key readKeys()
        {
            StreamReader r = new StreamReader("keys.json");
            string json = r.ReadToEnd();
            Dictionary<string, Key> keys = JsonConvert.DeserializeObject<Dictionary<string, Key>>(json);
            Console.WriteLine(keys["GDAX"].passphrase);
            r.Close();
            return keys["GDAX"];
        }

        public static Task<HttpResponseMessage> Get(string url, string data = "", Dictionary<string, string> parameters = null, bool auth = false)
        {
            if (parameters != null)
            {
                url += "?" + parameters.Aggregate(
                 new StringBuilder(),
                 (sb, pair) => sb.AppendFormat("{0}={1}&", pair.Key, pair.Value),
                 (sb) => sb.ToString());
                // Console.WriteLine("GET: " + url);
            }

            Dictionary<string, string> headers = new Dictionary<string, string>();
            if (auth) // format auth headers
            {
                Key key = readKeys();
                var timestamp = DateTime.UtcNow.ToTimestamp();
                var signedSignature = ComputeSignature(HttpMethod.Get, key.secret, timestamp, "/products/BTC-USD/candles");
                headers.Add("User-Agent", "CoinbaseProClient");
                headers.Add("CB-ACCESS-KEY", key.key);
                headers.Add("CB-ACCESS-TIMESTAMP", timestamp.ToString("F0", CultureInfo.InvariantCulture));
                headers.Add("CB-ACCESS-SIGN", key.secret);
                headers.Add("CB-ACCESS-PASSPHRASE", key.passphrase);
            }

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
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

        // public static Task<HttpResponseMessage> Post(string url, string data = "", Dictionary<string, string> headers = null, Dictionary<string, string> parameters = null)
        // {
        //     if (parameters != null)
        //     {
        //         url += parameters.Aggregate(
        //          new StringBuilder(),
        //          (sb, pair) => sb.AppendFormat("{0}={1}&", pair.Key, pair.Value),
        //          (sb) => sb.ToString());
        //     }

        //     HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
        //     if (data != String.Empty)
        //         request.Content = new StringContent(data, Encoding.UTF8);
        //     foreach (KeyValuePair<string, string> p in headers)
        //         request.Headers.Add(p.Key, p.Value);
        //     request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));
        //     return client.SendAsync(request);
        // }

        // public Requests(string url = "")
        // {
        //     Headers = new Dictionary<string, string>();
        //     Parameters = new Dictionary<string, string>();
        //     this.Url = url;
        //     this.Data = "";
        //     this.ContentType = "text/plain";
        // }

        // private Uri GenerateUri()
        // {
        //     return new Uri(this.Url +
        //         this.Parameters.Aggregate(
        //             new StringBuilder(),
        //             (sb, pair) => sb.AppendFormat("{0}={1}&", pair.Key, pair.Value),
        //             (sb) => sb.ToString()));
        // }
    }
}
