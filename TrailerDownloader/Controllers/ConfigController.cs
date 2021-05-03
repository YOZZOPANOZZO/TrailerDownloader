using Microsoft.AspNetCore.Mvc;
using TrailerDownloader.Models;
using TrailerDownloader.Repositories;
using TrailerDownloader.Services;

namespace TrailerDownloader.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private readonly IConfigRepository _configRepository;
        private readonly IAutoDownloadService _autoDownloadService;

        public ConfigController(IConfigRepository configRepository, IAutoDownloadService autoDownloadService)
        {
            _configRepository = configRepository;
            _autoDownloadService = autoDownloadService;
        }

        // GET: api/<ConfigController>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_configRepository.GetConfig());
        }

        // POST api/<ConfigController>
        [HttpPost]
        public IActionResult Post(Config config)
        {
            if (config.AutoTrailerDownload)
            {
                _autoDownloadService.StartAutoDownload();
            }

            return Ok(_configRepository.SaveConfig(config));
        }
    }
}
