using System.Threading.Tasks;

namespace TrailerDownloader.Services
{
    public interface IAutoDownloadService
    {
        Task StartAutoDownload();
    }
}
