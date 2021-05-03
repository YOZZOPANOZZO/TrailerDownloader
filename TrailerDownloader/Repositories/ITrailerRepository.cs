using System.Collections.Generic;
using System.Threading.Tasks;
using TrailerDownloader.Models;

namespace TrailerDownloader.Repositories
{
    public interface ITrailerRepository
    {
        Task<IEnumerable<Movie>> GetAllMoviesInfo(bool sendToClient = true);
        Task DownloadAllTrailers(IEnumerable<Movie> movieList, bool sendToClient = true);
        bool DeleteAllTrailers(IEnumerable<Movie> movieList);
    }
}
