using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.IO;

namespace X1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CodeController : ControllerBase
    {
        private readonly ILogger<ChartController> _logger;

        public CodeController(ILogger<ChartController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get([FromQuery(Name = "fname")] string fname)
        {
            StreamReader r = new StreamReader(fname);
            string source = r.ReadToEnd();
            return source;
        }


        [HttpPost]
        public void Post([FromQuery(Name = "fname")] string fname, [FromQuery(Name = "source")] string source)
        {
            using (StreamWriter writer = new StreamWriter(fname, false))
            {
                writer.Write(source);
            }

            // return Utf8Json.JsonSerializer.Deserialize<MemCache>(jsonContent);
        }
    }
}
