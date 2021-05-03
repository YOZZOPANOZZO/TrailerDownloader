using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrailerDownloader.Models;
using TrailerDownloader.Repositories;

namespace TrailerDownloader.Services
{
    public class AutoDownloadService : IAutoDownloadService
    {
        private readonly ILogger<AutoDownloadService> _logger;
        private readonly ITrailerRepository _trailerRepository;

        public AutoDownloadService(ILogger<AutoDownloadService> logger, ITrailerRepository trailerRepository)
        {
            _logger = logger;
            _trailerRepository = trailerRepository;
        }

        public Task StartAutoDownload()
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        IEnumerable<Movie> movieList = await _trailerRepository.GetAllMoviesInfo(false);
                        await _trailerRepository.DownloadAllTrailers(movieList, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error in AutoDownloadService.StartAutoDownload", ex);
                    }
                }
            });
        }
    }

}