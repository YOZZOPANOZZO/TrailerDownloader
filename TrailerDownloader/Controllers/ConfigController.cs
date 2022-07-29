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
        private readonly IConfigService _configService;
        private readonly IConfigRepository _configRepository;

        public ConfigController(IConfigService configService, IConfigRepository configRepository)
        {
            _configService = configService;
            _configRepository = configRepository;
        }

        // GET: api/<ConfigController>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_configRepository.GetConfig());
        }

        // POST api/<ConfigController>
        [HttpPost]
        public IActionResult Post(Config configs)
        {
            return Ok(_configService.SaveConfig(configs));
        }
    }
}
