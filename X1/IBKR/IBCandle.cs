namespace X1.IBKR
{
    public class IBCandle
    {

        /**
         * @brief The bar's date and time (either as a yyyymmss hh:mm:ss formatted string or as system time according to the request). Time zone is the TWS time zone chosen on login. 
         */
        public string time { get; set; }

        /**
         * @brief The bar's open price
         */
        public decimal open { get; set; }

        /**
         * @brief The bar's high price
         */
        public decimal high { get; set; }

        /**
         * @brief The bar's low price
         */
        public decimal low { get; set; }

        /**
         * @brief The bar's close price
         */
        public decimal close { get; set; }

        /**
         * @brief The bar's traded volume if available (only available for TRADES) 
         */
        public decimal volume { get; set; }

        /**
         * @brief The bar's Weighted Average Price (only available for TRADES) 
         */
        public decimal WAP { get; set; }

        /**
         * @brief The number of trades during the bar's timespan (only available for TRADES) 
         */
        public int count { get; set; }
    }

}