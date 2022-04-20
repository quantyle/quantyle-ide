using System.Collections.Generic;
using Newtonsoft.Json;

namespace X1
{

    // public class Product
    // {
    //     public string exchange { get; set; }
    //     public string productId { get; set; }
    //     public string baseAsset { get; set; }
    //     public string quoteAsset { get; set; }
    // }
    public class CoinbaseSubscription
    {
        public string type { get; set; } = "subscribe";
        public List<string> product_ids { get; set; }
        public List<string> channels { get; set; }
        public string signature { get; set; }
        public string key { get; set; }
        public string passphrase { get; set; }
        public string timestamp { get; set; }
    }


    public class Key
    {
        public List<string> permissions { get; set; }
        public string key { get; set; }
        public string passphrase { get; set; }
        public string secret { get; set; }
    }

    public class ChartResponse
    {
        public List<List<decimal>> data { get; set; }
    }

    public class Candle
    {
        public long time { get; set; }
        public decimal low { get; set; }
        public decimal high { get; set; }
        public decimal open { get; set; }
        public decimal close { get; set; }
        public decimal volume { get; set; }
    }

    public class Chart
    {
        public string exchange { get; set; }
        public string productId { get; set; }
        public List<List<decimal>> Data { get; set; }

    }

    // =========== GDAX ============
    // public class Trade
    // {
    //     public string id { get; set; }
    //     public string time { get; set; }
    //     public string price { get; set; }
    //     public string size { get; set; }
    //     public string side { get; set; }
    // }
    public class CoinbaseTrades
    {
        public List<CoinbaseTrades> data { get; set; }
    }

    public class CoinbaseAccounts
    {
        public IList<Dictionary<string, object>> data { get; set; }
    }


    public class CoinbaseProfiles
    {
        public IList<Dictionary<string, string>> data { get; set; }
    }


    public class CoinbaseFees
    {
        public string maker_fee_rate { get; set; }
        public string taker_fee_rate { get; set; }
        public string usd_volume { get; set; }

    }

    // =========== BINA ============
    public class BinanceChartResponse
    {
        public IList<IList<object>> data { get; set; }
    }
    // {
    //     "id":28706679,
    //     "price":"38907.1300",
    //     "qty":"0.00442000",
    //     "quoteQty":"171.9695",
    //     "time":1642740134514,
    //     "isBuyerMaker":true,
    //     "isBestMatch":true
    // }

    public class BinanceTrade
    {
        public decimal id { get; set; }
        public string price { get; set; }
        public string qty { get; set; }
        public string quoteQty { get; set; }
        public decimal time { get; set; }
        public bool isBuyerMaker { get; set; }
        public bool isBestMatch { get; set; }
    }
    public class BinanceTrades
    {
        public IList<BinanceTrade> data { get; set; }
    }


    public class BinanceBookSnapshot
    {
        public long date { get; set; }
        public decimal low { get; set; }
        public decimal high { get; set; }
        public decimal open { get; set; }
        public decimal close { get; set; }
        public decimal volume { get; set; }
    }

    public class BinanceAccount
    {
        public int makerCommission { get; set; }
        public int takerCommission { get; set; }
        public IList<Dictionary<string, string>> balances { get; set; }
    }

    // =========== KRKN ============
    public class KrakenBalances
    {
        public IList<string> error { get; set; }

        public Dictionary<string, string> result { get; set; }
    }


    public class KrakenAssetInfo
    {
        public IList<string> error { get; set; }

        public Dictionary<string, Dictionary<string, object>> result { get; set; }

        public string altname { get; set; }
        public string wsname { get; set; }

        [JsonProperty("base")]
        public string base_currency { get; set; }

        public IList<IList<decimal>> fees { get; set; }
        public IList<IList<decimal>> fees_maker { get; set; }


    }

    public class KrakenResponse
    {
        public IList<string> error { get; set; }

        public Dictionary<string, object> result { get; set; }
    }

    public class KrakenFees
    {
        public IList<string> error { get; set; }

        public Dictionary<string, object> result { get; set; }
    }

    // =========== GMNI ============

    public class GeminiBalance
    {
        public string type { get; set; }
        public string currency { get; set; }
        public string amount { get; set; }
        public string available { get; set; }
        public string availableForWithdrawal { get; set; }
    }

    public class GeminiFees
    {
        public int web_maker_fee_bps { get; set; }
        public int web_taker_fee_bps { get; set; }
        public int web_auction_fee_bps { get; set; }
        public int api_maker_fee_bps { get; set; }
        public int api_taker_fee_bps { get; set; }
        public int api_auction_fee_bps { get; set; }
    }

    public class Balance
    {
        public string exchange { get; set; }
        public string currency { get; set; }
        public string altName { get; set; } = ""; // mainly for KRKN, and its whacky naming conventions
        public decimal amount { get; set; } = 0;
        public decimal available { get; set; } = 0;
    }

    public class ProductBalance
    {
        public string exchange { get; set; }
        public string productId { get; set; }
        public decimal baseAmount { get; set; }
        public decimal baseAvailable { get; set; }
        public decimal quoteAmount { get; set; }
        public decimal quoteAvailable { get; set; }
    }

}
