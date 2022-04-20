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
    [Route("[controller]")]  // http://localhost:5000/weatherforecast
    public class PortfolioController : ControllerBase
    {

        private readonly ILogger<ChartController> _logger;

        public PortfolioController(ILogger<ChartController> logger)
        {
            _logger = logger;
        }

        // public Portfolio readPortfolioFile()
        // {
        //     string json = System.IO.File.ReadAllText("portfolio.json");
        //     // Console.WriteLine(portfolio.data["GDAX"][0]);
        //     return Newtonsoft.Json.JsonConvert.DeserializeObject<Portfolio>(json);
        // }

        // public string readPortfolioFile()
        // {
        //     return System.IO.File.ReadAllText("portfolio.json");
        // }


        [HttpGet]
        public Dictionary<string, string[]> Get()
        {
            string json = System.IO.File.ReadAllText("my_portfolio.json");
            var my_portfolio = Utf8Json.JsonSerializer.Deserialize<Dictionary<string, string[]>>(json);
            
            return my_portfolio;
        }
    }
}
