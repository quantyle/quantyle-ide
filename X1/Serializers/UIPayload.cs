using System.Collections.Generic;

namespace X1
{
    public class Book
    {
        public decimal[,] asks { get; set; } = new decimal[,] {
                { 0, 0 },
                { 0, 0 },
                { 0, 0 },
                { 0, 0 },
                { 0, 0 },
                { 0, 0 },
                { 0, 0 },
                { 0, 0 },
                { 0, 0 },
                { 0, 0 },

                // { 0, 0 },
                // { 0, 0 },
                // { 0, 0 },
                // { 0, 0 },
                // { 0, 0 },
                };
        public decimal[,] bids { get; set; } = new decimal[,] {
                { 0, 0 },
                { 0, 0 },
                { 0, 0 },
                { 0, 0 },
                { 0, 0 },
                { 0, 0 },
                { 0, 0 },
                { 0, 0 },
                { 0, 0 },
                { 0, 0 },

                // { 0, 0 },
                // { 0, 0 },
                // { 0, 0 },
                // { 0, 0 },
                // { 0, 0 },
                };
        public decimal volume { get; set; } = 0;
    }

    public class BookIB
    {
        public List<List<decimal>> asks { get; set; }
        public List<List<decimal>> bids { get; set; }
        // total volume for given depth, NOT entire order book
        public decimal volume { get; set; }
    }

    public class Ticker
    {
        public decimal id { get; set; }
        public decimal time { get; set; }
        public decimal price { get; set; }
        public decimal size { get; set; }
        public string side { get; set; }

    }

    public class OHLCV
    {
        public long time { get; set; }
        public decimal open { get; set; }
        public decimal high { get; set; }
        public decimal low { get; set; }
        public decimal close { get; set; }
        public decimal volume { get; set; }
    }


    public class UIPayload
    {
        public string type = "feed";
        public long count { get; set; } = 1;
        public decimal portfolioValue { get; set; } = 0;
        public decimal usdAvailable { get; set; } = 0;
        public Book book { get; set; } = new Book();
        public Ticker ticker { get; set; } = new Ticker
        {
            time = 0,
            price = 0,
            size = 0,
        };
        public List<Ticker> tickers { get; set; } = new List<Ticker> { };
        public List<decimal> ohlcv { get; set; } = new List<decimal> { };
        public List<Ticker> snapshot { get; set; } = new List<Ticker> { };
        public List<List<decimal>> candles { get; set; } = new List<List<decimal>> { };

    }
    public class UIPayloadIB
    {
        public long count { get; set; } = 1;
        public decimal portfolioValue { get; set; }
        public BookIB book { get; set; } = new BookIB
        {
            asks = new List<List<decimal>> {
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },

                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                // new List<decimal>{ 0, 0 },
                // new List<decimal>{ 0, 0 },
                // new List<decimal>{ 0, 0 },
                // new List<decimal>{ 0, 0 },
                // new List<decimal>{ 0, 0 },
                },
            bids = new List<List<decimal>> {
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },

                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                // new List<decimal>{ 0, 0 },
                // new List<decimal>{ 0, 0 },
                // new List<decimal>{ 0, 0 },
                // new List<decimal>{ 0, 0 },
                // new List<decimal>{ 0, 0 },
                },
            volume = 0,
        };
        public Ticker ticker { get; set; } = new Ticker
        {
            time = 0,
            price = 0,
            size = 0,
        };
        public List<Ticker> tickers { get; set; } = new List<Ticker> { };
        public List<decimal> ohlcv { get; set; } = new List<decimal>();
        public List<Ticker> snapshot { get; set; }

        public List<List<decimal>> candles { get; set; }
        //public UIPayload(){}

        public List<decimal> ohlcv2 { get; set; }
    }

    public class Portfolio
    {
        public Dictionary<string, List<string>> data { get; set; }
    }

}