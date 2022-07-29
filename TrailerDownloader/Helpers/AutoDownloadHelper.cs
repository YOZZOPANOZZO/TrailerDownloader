using System;
using System.Threading.Tasks;
using TrailerDownloader.SignalRHubs;

namespace TrailerDownloader.Helpers
{
    public static class AutoDownloadHelper
    {
        public static void Start()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    var movieHub = new MovieHub();
                    var movies = await movieHub.GetAllMoviesInfo();
                    movieHub.DownloadAllTrailers(movies);

                    await Task.Delay(TimeSpan.FromMinutes(2));
                }
            });
        }
    }
}