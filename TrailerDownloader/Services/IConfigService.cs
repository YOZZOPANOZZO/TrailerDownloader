using TrailerDownloader.Models;

namespace TrailerDownloader.Services
{
    public interface IConfigService
    {
        bool SaveConfig(Config config);
    }
}