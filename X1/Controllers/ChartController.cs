using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace X1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChartController : ControllerBase
    {
        private readonly ILogger<ChartController> _logger;

        public ChartController(ILogger<ChartController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public Chart Get([FromQuery(Name = "exchange")] string exchange, [FromQuery(Name = "symbol")] string symbol)
        {
            List<List<decimal>> data = new List<List<decimal>>();
            if (exchange == "KRKN")
            {
                Clients.Kraken kraken = new Clients.Kraken();
                data = kraken.getChartList(symbol);
            }
            else if (exchange == "GDAX")
            {
                Clients.Coinbase cb = new Clients.Coinbase();
                data = cb.getChartList(symbol);
            }
            else if (exchange == "BINA")
            {
                Clients.Binance binance = new Clients.Binance();
                data = binance.getChartList(symbol);
            }
            else if (exchange == "GMNI")
            {
                Clients.Gemini gemini = new Clients.Gemini();
                data = gemini.getChartList(symbol);
            }
            return new Chart { productId = symbol, exchange = exchange, Data = data };

        }
    }
}
