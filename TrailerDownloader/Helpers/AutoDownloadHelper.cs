using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using TrailerDownloader.SignalRHubs;

namespace TrailerDownloader.Helpers
{
    public static class AutoDownloadHelper
    {
        public static bool LoadedFirstTime { get; set; }

        public static void Start(IHubContext<MovieHub> hubContext)
        {
            Task.Run(async () =>
            {
                // var firstTime = true;
                
                while (true)
                {
                    if (!LoadedFirstTime)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        continue;
                    }

                    // if (firstTime)
                    //     await Task.Delay(TimeSpan.FromSeconds(30));

                    // firstTime = false;
                    
                    Log.Information("Auto download started");
                    
                    var movieHub = new MovieHub(hubContext);
                    var movies = await movieHub.GetAllMoviesInfo(false);
                    movieHub.DownloadAllTrailers(movies, false);

                    await Task.Delay(TimeSpan.FromSeconds(15));
                }
            });
        }
    }
}