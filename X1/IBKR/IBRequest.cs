namespace X1.IBKR
{
    // DO NOT CHANGE THE ORDER OF THESE PARAMETERS (MESSSAGE ENCODING DEPENDS ON ORDER)
    public class IBReqMktDepth
    {
        public int reqType { get; set; }
        public int VERSION { get; set; }
        public int reqId { get; set; }
        public int conId { get; set; }
        public string symbol { get; set; }
        public string secType { get; set; }
        public string lastTradeDateOrContractMonth { get; set; }
        public double strike { get; set; }
        public string right { get; set; }
        public string multiplier { get; set; }
        public string exchange { get; set; }
        public string primaryExchange { get; set; }
        public string currency { get; set; }
        public string localSymbol { get; set; }
        public string tradingClass { get; set; }
        public int numRows { get; set; } // the number of rows on each side of the order book
        public int isSmartDepth { get; set; }  // flag indicates that this is smart depth request
        public string mktDataOptions { get; set; }
    }


    public class IBReqAccountSummary
    {
        public int reqType { get; set; }
        public int VERSION { get; set; }
        public int reqId { get; set; }
        public string group { get; set; }
        public string tags { get; set; }
    }
    public class IBReqMktData
    {
        public int reqType { get; set; }
        public int VERSION { get; set; }
        public int reqId { get; set; }
        public int conId { get; set; }
        public string symbol { get; set; }
        public string secType { get; set; }
        public string lastTradeDateOrContractMonth { get; set; }
        public double strike { get; set; }
        public string right { get; set; }
        public string multiplier { get; set; }
        public string exchange { get; set; }
        public string primaryExchange { get; set; }
        public string currency { get; set; }
        public string localSymbol { get; set; }
        public string tradingClass { get; set; }
        public string internalUseOnly { get; set; } = "0";
        public string genericTickList { get; set; }
        public int snapshot { get; set; }
        public int regulatorySnapshot { get; set; }
        public string mktDataOptions { get; set; }
    }

    public class IBReqHistoricalTicks
    {
        public int reqType { get; set; }
        public int reqId { get; set; }
        public int conId { get; set; }
        public string symbol { get; set; }
        public string secType { get; set; }
        public string lastTradeDateOrContractMonth { get; set; }
        public double strike { get; set; }
        public string right { get; set; }
        public string multiplier { get; set; }
        public string exchange { get; set; }
        public string primaryExchange { get; set; }
        public string currency { get; set; }
        public string localSymbol { get; set; }
        public string tradingClass { get; set; }
        public int includeExpired { get; set; }
        public string startDateTime { get; set; }  // "20170701 12:01:00". Uses TWS timezone specified at login.
        public string endDateTime { get; set; }  // "20170701 13:01:00". In TWS timezone. Exactly one of start time and end time has to be defined.
        public int numberOfTicks { get; set; }  // Number of distinct data points. Max currently 1000 per request.
        public string whatToShow { get; set; } // (Bid_Ask, Midpoint, Trades) Type of data requested.
        public int useRTH { get; set; } //	set to 0 to obtain the data which was also generated outside of the Regular Trading Hours, set to 1 to obtain only the RTH data
        public string ignoreSize { get; set; }  // 	A filter only used when the source price is Bid_Ask
        public string miscOptions { get; set; } // should be defined as null, reserved for internal use
    }

    public class IBReqHistoricalData
    {
        public int reqType { get; set; }
        public int reqId { get; set; }
        public int conId { get; set; }
        public string symbol { get; set; }
        public string secType { get; set; }
        public string lastTradeDateOrContractMonth { get; set; }
        public double strike { get; set; }
        public string right { get; set; }
        public string multiplier { get; set; }
        public string exchange { get; set; }
        public string primaryExchange { get; set; }
        public string currency { get; set; }
        public string localSymbol { get; set; }
        public string tradingClass { get; set; }
        public int includeExpired { get; set; }
        public string endDateTime { get; set; }
        public string barSizeSetting { get; set; }  // 1 sec, 5, secs, 15 secs, 20 secs, 1 min, 2 mins, 2 mins, 5 mins, 15 mins, 30 mins, 1 hour, 1 day
        public string durationStr { get; set; }  // S, D, W, M, Y (seconds, days, weeks, months, years)
        public int useRTH { get; set; } //	set to 0 to obtain the data which was also generated outside of the Regular Trading Hours, set to 1 to obtain only the RTH data
        public string whatToShow { get; set; } // TRADES, MIDPOINT, BID, ASK, BID_ASK, HISTORICAL_VOLATILITY, OPTION_IMPLIED_VOLATILITY, FEE_RATE, REBATE_RATE
        public int formatDate { get; set; }  // set to 1 to obtain the bars' time as yyyyMMdd HH:mm:ss, set to 2 to obtain it like system time format in seconds
        public int keepUpToDate { get; set; }  // set to True to received continuous updates on most recent bar data. If True, and endDateTime cannot be specified.
        public string chartOptions { get; set; }
    }


    public class IBReqTickByTick
    {
        public int reqType { get; set; }
        public int reqId { get; set; }
        public int conId { get; set; }
        public string symbol { get; set; }
        public string secType { get; set; }
        public string lastTradeDateOrContractMonth { get; set; }
        public double strike { get; set; }
        public string right { get; set; }
        public string multiplier { get; set; }
        public string exchange { get; set; }
        public string primaryExchange { get; set; }
        public string currency { get; set; }
        public string localSymbol { get; set; }
        public string tradingClass { get; set; }
        public string tickType { get; set; }  //  tick-by-tick data type: "Last", "AllLast", "BidAsk" or "MidPoint".
        public int numberOfTicks { get; set; }  // number of ticks.
        public string ignoreSize { get; set; }

    }
}
