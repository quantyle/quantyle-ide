using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using WebSocketSharp.Server;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace X1
{
    public class Dojo
    {
        private int updateFreqMilliseconds = 75;
        private int tickSnapshotSize = 300;
        private const string websocketHost = "ws://127.0.0.1:8004";
        private string portfolioFile = "my_portfolio.json";
        private string folderName = "./media/data";
        private Clients.Coinbase coinbaseCli = new Clients.Coinbase();
        private Clients.Binance binanceCli = new Clients.Binance();
        private Clients.Gemini geminiCli = new Clients.Gemini();
        private Clients.Kraken krakenCli = new Clients.Kraken();
        private Dictionary<string, Dictionary<string, Balance>> exchangeBalances = new Dictionary<string, Dictionary<string, Balance>>();
        private string[] exchanges = new string[] { "GDAX", "BINA", "GMNI", "KRKN" };
        private Dictionary<string, string[]> portfolio = new Dictionary<string, string[]>();
        private ConcurrentDictionary<string, Ticker> allTickers = new ConcurrentDictionary<string, Ticker>();

        private Dictionary<string, Task> processes = new Dictionary<string, Task>();

        public async Task run()
        {
            // loadBalances();
            loadMyPortfolio();
            createDataDirectory();

            decimal startTime = Files.getCurrentUnixMinute();
            decimal endTime = Convert.ToInt64(startTime) - (60 * 60 * 24);

            // load trades we've missed since endTime
            await loadHistoricTrades(startTime, endTime);

            // load websocket server
            var websocketServer = new WebSocketServer(websocketHost);
            var cts = new CancellationTokenSource();


            foreach (var exchange in this.portfolio.Keys)
            {
                foreach (var product in this.portfolio[exchange])
                {
                    string key = exchange + "-" + product;

                    // ProductBalance productBalance = loadProductBalance(exchange, productId);
                    allTickers.TryAdd(exchange + "-" + product, new Ticker());

                    Task exchangeTask = Task.Factory.StartNew(() => Exchange.run(
                        product,
                        exchange,
                        allTickers,
                        updateFreqMilliseconds,
                        tickSnapshotSize,
                        websocketServer,
                        cts), TaskCreationOptions.LongRunning);
                    // add thread to dictionary
                    processes.Add(key, exchangeTask);
                    Console.WriteLine("starting a long-running Task for: {0}-{1}", exchange, product);
                    Thread.Sleep(3000);
                }
            }


            // add the aggregate tickers feed to our websocketserver 
            websocketServer.AddWebSocketService<AllTicks>("/allTicks", () => new AllTicks(allTickers));

            // start websocket server
            websocketServer.Start();

            // launc HTTP server
            CreateHostBuilder().Build().Run();

            // // stop the local ws server
            websocketServer.Stop();

            // stop all threads
            cts.Cancel();
            // wait for threads to stop
            Thread.Sleep(1000);
        }


        public void loadBalances()
        {
            // loads the initial balances
            // get product balances
            var coinbaseBalances = coinbaseCli.getBalances();
            var geminiBalances = geminiCli.getBalances();
            var krakenBalances = krakenCli.getBalances();
            var binanceBalances = binanceCli.getBalances();

            // var exchangeBalances = new Dictionary<string, Dictionary<string, Balance>>();
            exchangeBalances.Add("GDAX", coinbaseBalances);
            exchangeBalances.Add("BINA", binanceBalances);
            exchangeBalances.Add("GMNI", geminiBalances);
            exchangeBalances.Add("KRKN", krakenBalances);
        }
        public void loadMyPortfolio()
        {
            string jsonContent = System.IO.File.ReadAllText(portfolioFile);
            portfolio = Utf8Json.JsonSerializer.Deserialize<Dictionary<string, string[]>>(jsonContent);
        }

        public IHostBuilder CreateHostBuilder() =>
            Host
                .CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    //webBuilder.UseSetting("CustomProperty", someProperty.ToString());
                });

        public ProductBalance loadProductBalance(string exchange, string product)
        {
            string baseCurrency = product.Split("-")[0];
            string quoteCurrency = product.Split("-")[1];
            ProductBalance productBalance = new ProductBalance
            {
                exchange = exchange,
                productId = product,
                baseAmount = 0,
                baseAvailable = 0,
                quoteAmount = 0,
                quoteAvailable = 0,
            };

            // find the base currency balance if it exists
            if (exchangeBalances[exchange].TryGetValue(baseCurrency, out Balance baseBalance))
            {
                productBalance.baseAvailable = baseBalance.available;
                productBalance.baseAmount = baseBalance.amount;
            }

            // find the quote currency balance if it exists
            if (exchangeBalances[exchange].TryGetValue(quoteCurrency, out Balance quoteBalance))
            {
                productBalance.quoteAvailable = quoteBalance.available;
                productBalance.quoteAmount = quoteBalance.amount;
            }
            return productBalance;
        }
        public void createDataDirectory()
        {
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
        }

        public async Task loadHistoricTrades(decimal startTime, decimal endTime)
        {
            // build a list of tasks to load historical trades
            List<Task> loadTradesTasks = new List<Task>();

            // load trades from respective APIs 
            foreach (var exchange in this.portfolio.Keys)
            {
                if (exchange == "GDAX")
                {
                    Task coinbaseLoad = Task.Run(() => coinbaseCli.loadPortfolioTrades(this.portfolio["GDAX"], startTime, endTime));
                    loadTradesTasks.Add(coinbaseLoad);
                }
                else if (exchange == "KRKN")
                {
                    Task krakenLoad = Task.Run(() => krakenCli.loadPortfolioTrades(this.portfolio["KRKN"], startTime, endTime));
                    loadTradesTasks.Add(krakenLoad);
                }
                else if (exchange == "GMNI")
                {
                    Task geminiLoad = Task.Run(() => geminiCli.loadPortfolioTrades(this.portfolio["GMNI"], startTime, endTime));
                    loadTradesTasks.Add(geminiLoad);
                }
                else if (exchange == "BINA")
                {
                    Task binanceLoad = Task.Run(() => binanceCli.loadPortfolioTrades(this.portfolio["BINA"], startTime, endTime));
                    loadTradesTasks.Add(binanceLoad);
                }
            }

            // wait for loading to finish
            await Task.WhenAll(loadTradesTasks);
            Console.WriteLine("Finished loading historic trades");
        }



        public List<List<decimal>> getChartList(string exchange, string product, decimal start = 0, decimal end = 0)
        {
            if (exchange == "GDAX")
            {
                return coinbaseCli.getChartList(product, start, end);
            }
            else if (exchange == "BINA")
            {
                return binanceCli.getChartList(product, start, end);
            }
            else if (exchange == "KRKN")
            {
                return krakenCli.getChartList(product);
            }
            else // GMNI
            {
                return geminiCli.getChartList(product);
            }
        }

        public void loadCharts(string exchange, string product)
        {
            // warning! ======= this only works for BINA & GDAX =======

            var chartList = getChartList(exchange, product); // used only to grab the chartList.Count
            var currentUnixTime = Files.getCurrentUnixMinute();
            string path = "./media/data/" + exchange + "-" + product + "-V2.csv"; // reversed 
            int delay = 150;
            int limit = chartList.Count;
            int days = 1;
            decimal endTime = Convert.ToInt64(currentUnixTime) - (60 * 60 * 24 * days);

            // if the file exists, update it
            if (File.Exists(path))
            {
                // remove the last line and get the last timestamp
                // Files.RemoveLastLine(path);
                string lastLine = Files.ReadLastLine(path);
                endTime = decimal.Parse(lastLine.Split(",")[0]) + (60 * limit) + 60;
                Console.WriteLine("updating charts-v1");
            }
            else
            {
                // if the file is not found, 
                Console.WriteLine("creating charts-v1 for {0}-{1}", exchange, product);
            }

            // if the file exists we append the text, otherwise we create the file
            using (StreamWriter sw = File.Exists(path) ? File.AppendText(path) : File.CreateText(path))
            {
                while (true)
                {
                    try
                    {
                        decimal startTime = endTime - (60 * limit);
                        chartList = getChartList(exchange, product, start: startTime, end: endTime);

                        for (int j = 0; j < chartList.Count; j++)
                        {
                            string line = String.Join(",", chartList[j].ToArray());
                            sw.WriteLine(line);
                        }
                        // sw.WriteLine("========");

                        if (chartList.Count == 0)
                        {
                            break;
                        }
                        endTime += (60 * limit);
                        Console.WriteLine("{0}-{1} > {2}", exchange, product, endTime);
                        Thread.Sleep(delay);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Thread.Sleep(2000);
                    }
                }
            }
        }
    }
}

