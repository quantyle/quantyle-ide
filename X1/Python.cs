using System;
using X1.Compiler;
using ValueType = X1.Compiler.ValueType;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
// using System.IO;
using System.Text;

namespace X1
{
    public class DojoResponse
    {
        public string type = "code";
        public List<List<decimal>> chart { get; set; }
        public List<string> lineColors { get; set; }
        public string printOutput { get; set; } = "";
        public string error { get; set; } = "";
        public int errorLineNumber { get; set; }
    }


    public class Python
    {

        int index;
        List<List<decimal>> candleSnapshot;
        RunTime run_time;
        Script script;
        Dictionary<int, string> indicatorColors = new Dictionary<int, string>();
        public StringBuilder printOutput = new StringBuilder();
        Dictionary<string, decimal> settings = new Dictionary<string, decimal>();
        Dictionary<decimal, decimal> prev_emas = new Dictionary<decimal, decimal>();

        // memory will be used to hold values we do not want to change as we iterate over the chart
        Dictionary<string, Type> memory = new Dictionary<string, Type>();


        public Python(string source, List<List<decimal>> chart)
        {
            // Console.WriteLine("============== PYTHON 1 ===========");
            byte[] serialized = Utf8Json.JsonSerializer.Serialize<List<List<decimal>>>(chart);
            candleSnapshot = Utf8Json.JsonSerializer.Deserialize<List<List<decimal>>>(serialized);
            var log_settings = new Script.LogSettings();
            log_settings.print_tokens = false;
            log_settings.print_intermediate_code = false;
            log_settings.print_statements = false;
            script = new Script(source, log_settings);
        }

        public void run()
        {
            settings.Add("crypto", 0);

            // for (var i = candleSnapshot.Count - 1; i >= 0; i--)
            for (var i = 0; i < candleSnapshot.Count; i++)
            {
                index = i;
                run_time = new RunTime(script);

                // misc. functions
                run_time.add_function("print", print);
                run_time.add_function("index", get_index);
                run_time.add_function("set_fee", set_fee);
                run_time.add_function("set_cash", set_cash);
                run_time.add_function("get_cash", get_cash);
                run_time.add_function("get_crypto", get_crypto);

                // indicator functions
                run_time.add_function("sma", sma);
                run_time.add_function("ema", ema);
                run_time.add_function("vwap", vwap);
                run_time.add_function("stdev", stdev);

                // execution functions:
                run_time.add_function("buy_market", buy_market);
                run_time.add_function("sell_market", sell_market);


                // Variable Initialization      
                // convert to doubles as Python does cannot use the C# decimal type
                double open = Convert.ToDouble(candleSnapshot[i][1]);
                double high = Convert.ToDouble(candleSnapshot[i][1]);
                double low = Convert.ToDouble(candleSnapshot[i][1]);
                double close = Convert.ToDouble(candleSnapshot[i][1]);
                double volume = Convert.ToDouble(candleSnapshot[i][1]);
                // calculate the typical price
                double typical = (high + low + close) / 3;


                run_time.symbol_table.store("open", new DynamicValue(open));
                run_time.symbol_table.store("high", new DynamicValue(high));
                run_time.symbol_table.store("low", new DynamicValue(low));
                run_time.symbol_table.store("close", new DynamicValue(close));
                run_time.symbol_table.store("volume", new DynamicValue(volume));
                run_time.symbol_table.store("typical", new DynamicValue(typical));


                // add some memory spaces to the chart for buy, sell, cash and crypto
                candleSnapshot[index].Add(0); // buy  (int as boolean)
                candleSnapshot[index].Add(0);  // sell (int as boolean)
                candleSnapshot[index].Add(0); // cash amount
                candleSnapshot[index].Add(0);  // crypto amount

                decimal PnL = 0;
                if (settings.ContainsKey("cash"))
                {
                    PnL = (Convert.ToDecimal(close) * settings["crypto"]) + settings["cash"];
                }

                candleSnapshot[index].Add(PnL);  // total portfolio value (PnL)

                // execute
                run_time.run();
            }
            Console.WriteLine("finished executing python script.");
        }

        Value print(FunctionArguments arguments, SymbolTable symbol_table)
        {
            string sep = arguments.get_string_argument(null, "sep", " ", symbol_table);
            var list_arg = arguments.get_list_arguments(0, symbol_table);

            for (int i = 0; i < list_arg.Count; i++)
            {
                printOutput.Append(list_arg[i].ToString());

                if (i < list_arg.Count - 1)
                    printOutput.Append(sep);
            }
            printOutput.Append("\n");
            // Console.WriteLine(printOutput);
            return NoneValue.NONE;
        }


        Value set_fee(FunctionArguments arguments, SymbolTable symbol_table)
        {
            arguments.check_num_args(1);

            if (!settings.ContainsKey("fee"))
            {
                var fee = Convert.ToDecimal(arguments.get_double_argument(0, "fee", null, symbol_table));
                Console.WriteLine(fee);
                settings.Add("fee", fee);
                // candleSnapshot[index][8] = fee;
            }
            return NoneValue.NONE;
        }

        Value set_cash(FunctionArguments arguments, SymbolTable symbol_table)
        {
            // ERROR: CURRENTLY THIS ONLY SUPPORT INTEGER CASH VALUES

            // Console.WriteLine("attempting to set cash");
            arguments.check_num_args(1);

            if (!settings.ContainsKey("cash"))
            {
                var cash = Convert.ToDecimal(arguments.get_double_argument(0, "cash", null, symbol_table));
                Console.WriteLine(cash);
                settings.Add("cash", cash);
                candleSnapshot[index][8] = cash;
            }
            return NoneValue.NONE;
        }

        Value get_cash(FunctionArguments arguments, SymbolTable symbol_table)
        {
            double cash_value = 0;
            if (settings.ContainsKey("cash"))
            {
                cash_value = Convert.ToDouble(settings["cash"]);
            }
            return new DynamicValue(cash_value);
        }


        Value get_crypto(FunctionArguments arguments, SymbolTable symbol_table)
        {
            double crypto_value = 0;
            if (settings.ContainsKey("crypto"))
            {
                crypto_value = Convert.ToDouble(settings["crypto"]);
            }
            return new DynamicValue(crypto_value);
        }

        Value sell_market(FunctionArguments arguments, SymbolTable symbol_table)
        {
            arguments.check_num_args(1); // only required to have size or percent
            decimal size = Convert.ToDecimal(arguments.get_double_argument(null, "size", 0, symbol_table));
            decimal percent = Convert.ToDecimal(arguments.get_double_argument(null, "percent", 0, symbol_table));

            if (settings.ContainsKey("cash"))
            {
                decimal close_price = candleSnapshot[index][4];
                if (size > 0)
                {
                    decimal value = (size * close_price);
                    // only sell if we end up with enough crypto to sell
                    if ((settings["crypto"] - size) >= 0)
                    {
                        if (settings.ContainsKey("fee"))
                        {
                            settings["cash"] -= (value * settings["fee"]); // subtract the cost of the fee
                        }
                        settings["cash"] += value; // add the value of the trade
                        settings["crypto"] -= size;
                        candleSnapshot[index][7] = 1; // sell = 1, or sell = true
                    }

                    // set cash & crypto (pnl)
                    candleSnapshot[index][8] = settings["cash"];
                    candleSnapshot[index][9] = settings["crypto"];
                }
            }
            else
            {
                Console.WriteLine("sell_market: NO CASH AVAILABLE");
            }
            return new DynamicValue(Convert.ToDouble(candleSnapshot[index][7]));
        }

        Value buy_market(FunctionArguments arguments, SymbolTable symbol_table)
        {
            arguments.check_num_args(1); // only required to have size or percent
            decimal size = Convert.ToDecimal(arguments.get_double_argument(null, "size", 0, symbol_table));
            decimal percent = Convert.ToDecimal(arguments.get_double_argument(null, "percent", 0, symbol_table));
            if (settings.ContainsKey("cash"))
            {
                decimal close_price = candleSnapshot[index][4];
                if (size > 0)
                {
                    decimal value = (size * close_price);
                    // only buy if we end up with enough cash to buy
                    if ((settings["cash"] - value) >= 0)
                    {
                        if (settings.ContainsKey("fee"))
                        {
                            settings["cash"] -= (value * settings["fee"]); // subtract the cost of the fee
                        }
                        settings["cash"] -= value; // subtract the cost of the trade

                        settings["crypto"] += size;
                        candleSnapshot[index][6] = 1; // buy = 1, or buy = true
                    }
                    // set cash & crypto (pnl)
                    candleSnapshot[index][8] = settings["cash"];
                    candleSnapshot[index][9] = settings["crypto"];
                }
            }
            else
            {
                Console.WriteLine("buy_market: NO CASH AVAILABLE");
            }
            return new DynamicValue(Convert.ToDouble(candleSnapshot[index][6]));
        }


        public DojoResponse getResponse()
        {
            // get the response to be sent to the frontend
            List<string> colors = new List<string>();
            for (var i = 0; i < indicatorColors.Keys.Count; i++)
            {
                colors.Add(indicatorColors[i]);
            }

            return new DojoResponse
            {
                chart = candleSnapshot,
                lineColors = colors,
                printOutput = printOutput.ToString(),
            };
        }

        void setIndicatorColor(string color)
        {
            // int indicatorKey = (candleSnapshot[index].Count - 9);
            int indicatorKey = (candleSnapshot[index].Count - 12);
            indicatorColors.TryAdd(indicatorKey, processColor(color));
        }


        decimal calc_sma(int period, int i)
        {
            decimal sma = -1;
            decimal sum = 0;
            // rolling window to calculate sum of closing prices
            for (var j = 0; j < period; j++)
            {
                sum += candleSnapshot[i - j][4];  // 4 --> we are using close value here
            }
            sma = sum / period;
            return sma;
        }

        decimal calc_total_volume(int period)
        {
            // calculate the total volume for a given period
            decimal total_volume = 0;
            for (var i = 0; i < period; i++)
            {
                total_volume += candleSnapshot[index - i][5];
            }
            return total_volume;
        }

        decimal calc_typical_price(int i)
        {
            // calculate the Typical Price = (H + C + L)/ 3
            decimal high = candleSnapshot[i][2];
            decimal low = candleSnapshot[i][3];
            decimal close = candleSnapshot[i][4];
            return (high + low + close) / 3;
        }


        // Value norm(FunctionArguments arguments, SymbolTable symbol_table)
        // {
        //     arguments.check_num_args(3);
        //     var id = arguments.get_string_argument(0, "id", null, symbol_table);
        //     var period = arguments.get_int_argument(1, "period", null, symbol_table);
        //     var color = arguments.get_string_argument(2, "color", null, symbol_table);
        //     decimal sma = -1;

        //     // we have completed 1 period
        //     if (index >= period)
        //     {
        //         sma = calc_sma(period, index);
        //     }
        //     else
        //     {
        //         sma = -1; // sma == close
        //     }

        //     // add to the chart
        //     candleSnapshot[index].Add(sma);

        //     setIndicatorColor(color);

        //     return new DynamicValue(Convert.ToDouble(sma));
        // }


        Value stdev(FunctionArguments arguments, SymbolTable symbol_table)
        {
            arguments.check_num_args(2);
            var period = arguments.get_int_argument(1, "period", null, symbol_table);
            var color = arguments.get_string_argument(2, "color", null, symbol_table);
            double stdev = -1;

            // we have completed 1 period
            if (index >= period)
            {
                // 1. calculate moving average
                decimal sma = calc_sma(period, index);

                double sum_of_avg_deviations = 0;
                for (var i = 0; i < period; i++)
                {
                    sum_of_avg_deviations += Math.Pow(Convert.ToDouble(candleSnapshot[index - i][4] - sma), 2);
                }

                stdev = Math.Sqrt(Convert.ToDouble(sum_of_avg_deviations / period));
                Console.WriteLine("stdev: {0}", stdev);
            }
            else
            {
                stdev = -1; // sma == close
            }

            // add to the chart
            candleSnapshot[index].Add(Convert.ToDecimal(stdev));

            setIndicatorColor(color);

            return new DynamicValue(stdev);
        }


        Value sma(FunctionArguments arguments, SymbolTable symbol_table)
        {
            arguments.check_num_args(2);
            var period = arguments.get_int_argument(0, "period", null, symbol_table);
            var color = arguments.get_string_argument(1, "color", null, symbol_table);
            decimal sma = -1;

            // we have completed 1 period
            if (index >= period)
            {
                sma = calc_sma(period, index);
            }
            else
            {
                sma = -1; // sma == close
            }

            // add to the chart
            candleSnapshot[index].Add(sma);

            setIndicatorColor(color);

            return new DynamicValue(Convert.ToDouble(sma));
        }

        Value ema(FunctionArguments arguments, SymbolTable symbol_table)
        {
            arguments.check_num_args(2);
            var period = arguments.get_int_argument(0, "period", null, symbol_table);
            var color = arguments.get_string_argument(1, "color", null, symbol_table);
            decimal sma = -1;
            decimal ema = -1;
            decimal smoothingK = 2;
            decimal multiplier = (smoothingK / (period + 1));

            if (index >= period)
            {
                decimal close = candleSnapshot[index][4];

                // if the key does not exist, perform our first calculation: EMA = close * multiplier + (sma * (1-multiplier))
                // we will use the period as the key, we should not need duplicate period EMAs
                if (!prev_emas.ContainsKey(period))
                {
                    sma = calc_sma(period, index - 1);
                    ema = (close * multiplier) + (sma * (1 - multiplier));
                }
                else
                {
                    ema = (close * multiplier) + (prev_emas[period] * (1 - multiplier));
                }
                prev_emas[period] = ema;
            }
            else
            {
                ema = -1;
            }
            // Console.WriteLine("{0}: {1}... prev: {2}", index, ema, prev_ema);

            // add to the chart
            candleSnapshot[index].Add(ema);

            setIndicatorColor(color);


            return new DynamicValue(Convert.ToDouble(ema));
        }


        Value vwap(FunctionArguments arguments, SymbolTable symbol_table)
        {
            arguments.check_num_args(2);
            // var id = arguments.get_string_argument(null, "id", null, symbol_table);
            var period = arguments.get_int_argument(0, "period", null, symbol_table);
            var color = arguments.get_string_argument(1, "color", null, symbol_table);
            decimal vwap = -1;
            if (index >= period)
            {
                decimal top = 0;
                decimal sum_of_volumes = 0;
                for (var j = 0; j < period; j++)
                {
                    top += calc_typical_price(index - j) * candleSnapshot[index - j][5]; // sum(typical_price * volume)
                    sum_of_volumes += candleSnapshot[index - j][5]; // sum(volume)
                }

                // avoid divide by zero errors, if we have no volume then return the typical price
                if (sum_of_volumes > 0)
                {
                    vwap = top / sum_of_volumes; // sum(typical_price * volume) / sum(volume)
                }
                else
                {
                    // if we have zero volume, then just return the typical price
                    vwap = calc_typical_price(index);
                }
            }
            else
            {
                vwap = -1;
            }

            // add to the chart
            candleSnapshot[index].Add(vwap);

            // candleSnapshotColors.TryAdd(id, color);
            setIndicatorColor(color);

            return new DynamicValue(Convert.ToDouble(vwap));

        }






        // Value sell_limit(FunctionArguments arguments, SymbolTable symbol_table)
        // {
        //     arguments.check_num_args(2);

        //     var limit_price = arguments.get_int_argument(null, "price", null, symbol_table);
        //     var size = arguments.get_int_argument(null, "size", null, symbol_table);
        //     var percent = arguments.get_int_argument(null, "percent", null, symbol_table);

        //     if (settings.ContainsKey("cash"))
        //     {
        //         settings["cash"] += (size * limit_price);
        //     }

        //     // add to chart
        //     candleSnapshot[index][7] = 1;

        //     return new DynamicValue(candleSnapshot[index][7]);
        // }


        // Value buy_limit(FunctionArguments arguments, SymbolTable symbol_table)
        // {
        //     arguments.check_num_args(2);

        //     var limit_price = arguments.get_int_argument(null, "price", null, symbol_table);
        //     var size = arguments.get_int_argument(null, "size", null, symbol_table);
        //     var percent = arguments.get_int_argument(null, "percent", null, symbol_table);

        //     if (settings.ContainsKey("cash"))
        //     {
        //         settings["cash"] -= (size * limit_price);
        //     }

        //     // add to chart
        //     candleSnapshot[index][6] = 1;

        //     return new DynamicValue(candleSnapshot[index][6]);
        // }


        // User defined function example - add two numbers
        // Value user_add(FunctionArguments arguments, SymbolTable symbol_table)
        // {
        //     arguments.check_num_args(2);
        //     var val1 = arguments.get_double_argument(0, null, null, symbol_table);
        //     var val2 = arguments.get_double_argument(1, null, null, symbol_table);

        //     return new DynamicValue(val1 + val2);
        // }

        Value get_index(FunctionArguments arguments, SymbolTable symbol_table)
        {
            return new DynamicValue(index);
        }

        // Object based programming example - Python side constructor.
        // Value UserSum(FunctionArguments arguments, SymbolTable symbol_table)
        // {
        //     arguments.check_num_args(1);
        //     var start = arguments.get_double_argument(0, null, null, symbol_table);
        //     return new UserSum(start);
        // }

        public string processColor(string color)
        {
            Dictionary<string, string> basicColors = new Dictionary<string, string>
            {
                ["black"] = "#000000",
                ["silver"] = "#C0C0C0",
                ["gray"] = "#808080",
                ["white"] = "#FFFFFF",
                ["maroon"] = "#800000",
                ["red"] = "#FF0000",
                ["purple"] = "#800080",
                ["fuchsia"] = "#FF00FF",
                ["green"] = "#008000",
                ["lime"] = "#00FF00",
                ["olive"] = "#808000",
                ["yellow"] = "#FFFF00",
                ["navy"] = "#000080",
                ["blue"] = "#0000FF",
                ["teal"] = "#008080",
                ["aqua"] = "#00FFFF",
            };

            string hexColor = "";
            if (basicColors.TryGetValue(color.ToLower(), out hexColor))
            {
                return hexColor;
            }
            else if (color.ToLower().Contains("#"))
            {
                return color.ToLower();
            }

            Random rand = new Random();
            string randomColor = basicColors.ElementAt(rand.Next(0, basicColors.Count)).Value;
            // default
            return randomColor;
        }

        // public class ExponentialMovingAverageIndicator
        // {
        //     private bool _isInitialized;
        //     private readonly int _lookback;
        //     private readonly double _weightingMultiplier;
        //     private double _previousAverage;

        //     public double Average { get; private set; }
        //     public double Slope { get; private set; }

        //     public ExponentialMovingAverageIndicator(int lookback)
        //     {
        //         _lookback = lookback;
        //         _weightingMultiplier = 2.0 / (lookback + 1);
        //     }

        //     public void AddDataPoint(double dataPoint)
        //     {
        //         if (!_isInitialized)
        //         {
        //             Average = dataPoint;
        //             Slope = 0;
        //             _previousAverage = Average;
        //             _isInitialized = true;
        //             return;
        //         }

        //         Average = ((dataPoint - _previousAverage) * _weightingMultiplier) + _previousAverage;
        //         Slope = Average - _previousAverage;

        //         //update previous average
        //         _previousAverage = Average;
        //     }
        // }
    }



    class UserSum : Value
    {
        double sum = 0;


        public UserSum(double start)
        {
            sum = start;
        }

        public Value call(string function_name, FunctionArguments arguments, SymbolTable symbol_table)
        {
            // user_sum.add(5)
            if (function_name.Equals("add"))
            {
                arguments.check_num_args(1);
                var val1 = arguments.get_double_argument(0, null, null, symbol_table);
                sum += val1;

                return NoneValue.NONE;
            }
            // user_sum.get()
            else if (function_name.Equals("get"))
            {
                arguments.check_num_args(0);
                return new DynamicValue(sum);
            }

            return null;
        }



        public ValueType Type { get { return ValueType.UserDefined; } }

        public bool Equals(Value other)
        {
            // This is needed to be able to use the class in a list.
            return false;
        }

        public Value operate(OperatorType op_type, Value val2)
        {
            // This is needed to be able to use the class in a list.
            return null;
        }

        public override int GetHashCode()
        {
            // This is needed to be able to use the class as a dictionary key.
            return base.GetHashCode();
        }


    }
}