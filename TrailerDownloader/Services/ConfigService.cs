using Microsoft.AspNetCore.SignalR;
using TrailerDownloader.Helpers;
using TrailerDownloader.Models;
using TrailerDownloader.Repositories;
using TrailerDownloader.SignalRHubs;

namespace TrailerDownloader.Services
{
    public class ConfigService : IConfigService
    {
        private readonly IConfigRepository _configRepository;
        private readonly IHubContext<MovieHub> _hubContext;

        public ConfigService(IConfigRepository configRepository, IHubContext<MovieHub> hubContext)
        {
            _configRepository = configRepository;
            _hubContext = hubContext;
        }

        public bool SaveConfig(Config config)
        {
            if (config.AutoDownload)
                AutoDownloadHelper.Start(_hubContext);

            return _configRepository.SaveConfig(config);
        }
    }
}