using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Collections.Generic;

namespace X1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileController : ControllerBase
    {
        private readonly ILogger<ChartController> _logger;

        public FileController(ILogger<ChartController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public Dictionary<string, string> Get([FromQuery(Name = "directory")] string directory)
        {
            string[] fileEntries = Directory.GetFiles(directory);
            List<string> fileNames = new List<string>();
            Dictionary<string, string> files = new Dictionary<string, string>();

            // get the filenames from fullpath
            foreach (var fullPath in fileEntries)
            {
                string fileName = Path.GetFileName(fullPath);
                files.Add(fileName, fullPath);
            }
            return files;
        }
    }
}
