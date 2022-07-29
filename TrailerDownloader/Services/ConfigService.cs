using TrailerDownloader.Helpers;
using TrailerDownloader.Models;
using TrailerDownloader.Repositories;

namespace TrailerDownloader.Services
{
    public class ConfigService : IConfigService
    {
        private readonly IConfigRepository _configRepository;

        public ConfigService(IConfigRepository configRepository)
        {
            _configRepository = configRepository;
        }

        public bool SaveConfig(Config config)
        {
            if (config.AutoDownload)
                AutoDownloadHelper.Start();

            return _configRepository.SaveConfig(config);
        }
    }
}