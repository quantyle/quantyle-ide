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

    public class MemCache
    {
        public string activePath { get; set; }
        public string activeUser { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class MemCacheController : ControllerBase
    {
        private readonly ILogger<MemCacheController> _logger;

        private MemCache readMemCache()
        {
            string jsonContent = System.IO.File.ReadAllText("memcache.json");
            Console.WriteLine("reading memcache: {0}", jsonContent);
            return Utf8Json.JsonSerializer.Deserialize<MemCache>(jsonContent);
        }

        public MemCacheController(ILogger<MemCacheController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public MemCache Get()
        {
            return readMemCache();
        }

        [HttpPost]
        public void Post([FromQuery(Name = "MemCache")] string activePath, [FromQuery(Name = "MemCache")] string activeUser)
        {
            string jsonContent = System.IO.File.ReadAllText("memcache.json");
            Console.WriteLine("reading memcache: {0}", jsonContent);
            var parsed = Utf8Json.JsonSerializer.Deserialize<MemCache>(jsonContent);
            parsed.activePath = activePath;
            parsed.activeUser = activeUser;
            // save the code to file
            using (StreamWriter writer = new StreamWriter("temp.satoshi", false))
            {
                writer.Write(Utf8Json.JsonSerializer.Serialize(parsed));
            }

            // return Utf8Json.JsonSerializer.Deserialize<MemCache>(jsonContent);
        }
    }
}
