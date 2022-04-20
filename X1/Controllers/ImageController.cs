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
    public class ImageController : ControllerBase
    {

        private readonly ILogger<ImageController> _logger;



        public ImageController(ILogger<ImageController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("{symbol}")]  
        public FileStream Get(string symbol)
        {
            string path = String.Format("./media/img/{0}", symbol);
            return System.IO.File.OpenRead(path);
        }
    }
}
