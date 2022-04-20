using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Collections.Generic;


namespace X1
{


    [DataContract]
    public class TradeHistory
    {

        /// <summary>
        /// Time that the trade was executed
        /// </summary>
        [DataMember(Name = "timestamp")]
        public long Timestamp;
        /// <summary>
        /// The time that the trade was executed in milliseconds
        /// </summary>
        [DataMember(Name = "timestampms")]
        public long TimestampMs;
        /// <summary>
        /// The trade ID number
        /// </summary>
        [DataMember(Name = "tid")]
        public long TradeId;
        /// <summary>
        /// The price the trade was executed at
        /// </summary>
        [DataMember(Name = "price")]
        public decimal Price;
        /// <summary>
        /// The amount that was traded
        /// </summary>
        [DataMember(Name = "amount")]
        public decimal Amount;
        /// <summary>
        /// Will always be "gemini"
        /// </summary>
        [DataMember(Name = "exchange")]
        public string Exchange;
        /// <summary>
        /// "buy": an ask was removed by an incoming buy order
        /// "sell": a bid was removed by an incoming sell order
        /// "auction": bulk trade
        /// </summary>
        [DataMember(Name = "type")]
        public string Type;
        /// <summary>
        /// Whether the trade was broken or not. Broken trades will not be displayed by default; use the include_breaks to display them.
        /// </summary>
        [DataMember(Name = "broken")]
        public string Broken;
    }

    [DataContract]
    public class GeminiMarketDataEvent
    {
        [DataMember(Name = "tid")]
        public decimal tid;

        [DataMember(Name = "type")]
        public string type;

        [DataMember(Name = "price")]
        public decimal price;

        [DataMember(Name = "amount")]
        public decimal size;

        [DataMember(Name = "makerSide")]
        public string makerSide;

        [DataMember(Name = "side")]
        public string side;

        [DataMember(Name = "reason")]
        public string reason;

        [DataMember(Name = "remaining")]
        public decimal remaining;

        [DataMember(Name = "delta")]
        public decimal delta;
    }

    [DataContract]
    public class GeminiMarketData
    {

        [DataMember(Name = "type")]
        public string type;

        [DataMember(Name = "timestamp")]
        public long timestamp;

        [DataMember(Name = "timestampms")]
        public long timestampms;

        [DataMember(Name = "events")]
        public GeminiMarketDataEvent[] events;
    }


    [DataContract]
    public class CoinbaseMarketData
    {

        [DataMember(Name = "type")]
        public string Type;

        [DataMember(Name = "trade_id")]
        public decimal id;

        [DataMember(Name = "changes")]
        public List<List<string>> Changes;

        [DataMember(Name = "asks")]
        public List<List<string>> Asks;

        [DataMember(Name = "bids")]
        public List<List<string>> Bids;

        [DataMember(Name = "price")]
        public decimal price;

        [DataMember(Name = "last_size")]
        public decimal lastSize;


        [DataMember(Name = "time")]
        public string time;
        [DataMember(Name = "side")]
        public string side;

        // user messages, for verified channels only

        [DataMember(Name = "user_id")]
        public string UserId;

    }



    public class BinanceData
    {
        public string e { get; set; }
    }

    // {"e":"depthUpdate","E":1634589602466,"s":"BTCUSD","U":1285504711,"u":1285504714,"b":[["61242.2000","0.00000000"]],
    // "a":[["61282.7700","0.08900000"],["61283.0400","0.00000000"],["61288.4600","0.00000000"]]}
    public class BinanceDepthUpdate
    {
        public string result { get; set; }
        public string e { get; set; }

        public long E { get; set; }

        public string s { get; set; }

        public long U { get; set; }

        public long u { get; set; }

        public List<List<string>> b { get; set; }

        public List<List<string>> a { get; set; }

    }

    // {"e":"trade","E":1634589602485,"s":"BTCUSD","t":24317489,"p":"61263.9000","q":"0.00000500","b":664459366,"a":664458995,"T":1634589602485,"m":false,"M":true}
    public class BinanceWSTrade
    {
        public string result { get; set; }
        public string e { get; set; }
        public long E { get; set; }
        public string s { get; set; }
        public string p { get; set; }
        public string q { get; set; }
        public long b { get; set; }
        public long a { get; set; }
        public bool M { get; set; }
        public bool m { get; set; }
        public long T { get; set; }
        public decimal t { get; set; }

    }


    [DataContract]
    public class KrakenTrade
    {
        // sample message :

        // [324,{"a":["41168.80000",0,"0.89410239"],"b":["41168.70000",0,"0.03000000"],"c":["41173.80000","0.00529718"],
        // "v":["1595.82501376","1884.42409519"],"p":["41761.66406","41674.69495"],"t":[18589,20716],
        // "l":["40772.20000","40772.20000"],"h":["42579.90000","42579.90000"],"o":["41016.00000","41785.90000"]},
        // "ticker","XBT/USD"]

        [DataMember(Name = "a")]
        public List<string> bestAsk;  // [price, wholeLotVolume, lotVolume]

        [DataMember(Name = "b")]
        public List<string> bestBid;  // [price, wholeLotVolume, lotVolume]

        [DataMember(Name = "c")]
        public List<string> close; // [price, lotVolume] 

        [DataMember(Name = "v")]
        public List<string> volume; // [today, last24Hours]

        [DataMember(Name = "p")]
        public List<string> vwap; // [today, last24Hours]

        [DataMember(Name = "t")]
        public List<string> numberOfTrades; // [today, last24Hours]

        [DataMember(Name = "l")]
        public List<string> lowPrice; // [today, last24Hours]

        [DataMember(Name = "h")]
        public List<string> highPrice; // [today, last24Hours]

        [DataMember(Name = "o")]
        public List<string> openPrice; // [today, last24Hours]

    }



    [DataContract]
    public class KrakenTicker
    {
        // sample message :

        // [324,{"a":["41168.80000",0,"0.89410239"],"b":["41168.70000",0,"0.03000000"],"c":["41173.80000","0.00529718"],
        // "v":["1595.82501376","1884.42409519"],"p":["41761.66406","41674.69495"],"t":[18589,20716],
        // "l":["40772.20000","40772.20000"],"h":["42579.90000","42579.90000"],"o":["41016.00000","41785.90000"]},
        // "ticker","XBT/USD"]

        [DataMember(Name = "a")]
        public List<string> bestAsk;  // [price, wholeLotVolume, lotVolume]

        [DataMember(Name = "b")]
        public List<string> bestBid;  // [price, wholeLotVolume, lotVolume]

        [DataMember(Name = "c")]
        public List<string> close; // [price, lotVolume] 

        [DataMember(Name = "v")]
        public List<string> volume; // [today, last24Hours]

        [DataMember(Name = "p")]
        public List<string> vwap; // [today, last24Hours]

        [DataMember(Name = "t")]
        public List<string> numberOfTrades; // [today, last24Hours]

        [DataMember(Name = "l")]
        public List<string> lowPrice; // [today, last24Hours]

        [DataMember(Name = "h")]
        public List<string> highPrice; // [today, last24Hours]

        [DataMember(Name = "o")]
        public List<string> openPrice; // [today, last24Hours]

    }


    public class KrakenBook
    {
        // sample message :

        // [320,{"a":[["41326.30000","0.39206101","1632942805.042274"]],"c":"2842706256"},"book-25","XBT/USD"]

        [DataMember(Name = "a")]
        public List<List<string>> asks;

        [DataMember(Name = "b")]
        public List<List<string>> bids;

        [DataMember(Name = "as")]
        public List<List<string>> askSnapshot;

        [DataMember(Name = "bs")]
        public List<List<string>> bidSnapshot;

    }


    public class CodeSubmission
    {
        public string type { get; set; }
        public string data { get; set; }
    }




    // universal order data contract
    [DataContract]
    public class Order
    {
        public string id;
        public string time;
        public string type;
        public string side;  // limit, market
        public string price;
        public string size;
        public string status;  // received, open, done
    }


    [DataContract]
    public class CoinbaseOrder
    {
        [DataMember(Name = "order_id")]
        public string orderId;

        [DataMember(Name = "order_type")]
        public string orderType;

        [DataMember(Name = "product_id")]
        public string productId;

        [DataMember(Name = "price")]
        public string price;

        [DataMember(Name = "remaining_size")]
        public string remainingSize;

        [DataMember(Name = "size")]
        public string size;

        [DataMember(Name = "side")]
        public string side;  // buy, sell

        [DataMember(Name = "type")]
        public string type;  // "received", "open", "done", "match"

        [DataMember(Name = "time")]
        public string time;  // "2022-02-23T21:19:03.741472Z"

        [DataMember(Name = "profile_id")]
        public string profileId;  // "2022-02-23T21:19:03.741472Z"

        [DataMember(Name = "user_id")]
        public string userId;  // "2022-02-23T21:19:03.741472Z"

        [DataMember(Name = "reason")]
        public string reason;  // "2022-02-23T21:19:03.741472Z"
    }


    [DataContract]
    public class CoinbaseOpenOrder
    {
        [DataMember(Name = "id")]
        public string id;


        [DataMember(Name = "product_id")]
        public string productId;

        [DataMember(Name = "price")]
        public string price;

        [DataMember(Name = "remaining_size")]
        public string remainingSize;

        [DataMember(Name = "size")]
        public string size;

        [DataMember(Name = "side")]
        public string side;  // buy, sell


        [DataMember(Name = "created_at")]
        public string time;  // "2022-02-23T21:19:03.741472Z"


    }


    [DataContract]
    public class GeminiOrder
    {


    }

    [DataContract]
    public class BinanceOrder
    {

    }

    [DataContract]
    public class KrakenOrder
    {

    }

    [DataContract]
    public class KrakenOpenOrder
    {
        [DataMember(Name = "opentm")]
        public string opentm;  

        [DataMember(Name = "side")]
        public string starttm;  

        [DataMember(Name = "side")]
        public string side;  // buy, sell


    }

}