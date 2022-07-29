using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TrailerDownloader.SignalRHubs;

namespace TrailerDownloader.Helpers
{
    public static class AutoDownloadHelper
    {
        public static void Start(IHubContext<MovieHub> hubContext)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    var movieHub = new MovieHub(hubContext);
                    var movies = await movieHub.GetAllMoviesInfo();
                    movieHub.DownloadAllTrailers(movies);

                    await Task.Delay(TimeSpan.FromMinutes(2));
                }
            });
        }
    }
}