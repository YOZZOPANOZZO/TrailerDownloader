﻿using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Flurl.Http;
using TrailerDownloader.Models;
using TrailerDownloader.Models.DTOs;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace TrailerDownloader.SignalRHubs
{
    public class MovieHub : Hub
    {
        private readonly ILogger<MovieHub> _logger;
        private static IHubContext<MovieHub> _hubContext;
        private static readonly ConcurrentDictionary<string, Movie> _movieDictionary = new();

        private static readonly string _apiKey = "e438e2812f17faa299396505f2b375bb";
        private static readonly string _configPath = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
        private static readonly List<string> _excludedFileExtensions = new() { ".srt", ".sub", ".sbv", ".ssa", ".SRT2UTF-8", ".STL", ".png", ".jpg", ".jpeg", ".png", ".gif", ".svg", ".tif", ".tif", ".txt", ".nfo" };
        private static string _mainMovieDirectory;
        private static string _trailerLanguage;
        private static readonly List<string> _movieDirectories = new();

        public MovieHub() { }

        public MovieHub(ILogger<MovieHub> logger, IHubContext<MovieHub> hubContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));

            if (File.Exists(_configPath))
            {
                string jsonConfig = File.ReadAllText(_configPath);
                Config config = JsonConvert.DeserializeObject<Config>(jsonConfig);
                _mainMovieDirectory = config.MediaDirectory;
                _trailerLanguage = config.TrailerLanguage;
            }
        }

        public async Task<List<Movie>> GetAllMoviesInfo()
        {
            GetMovieDirectories(_mainMovieDirectory);
            List<Task<Movie>> taskList = new List<Task<Movie>>();

            foreach (string movieDirectory in _movieDirectories)
            {
                foreach (string movieDirectory1 in Directory.GetDirectories(movieDirectory))
                {
                    Movie movie = GetMovieFromDirectory(movieDirectory1);
                    if (movie == null)
                    {
                        _logger.LogInformation($"No movie found in directory: '{movieDirectory1}'");
                        continue;
                    }

                    if (_movieDictionary.TryGetValue(movie.Title, out Movie dictionaryMovie))
                    {
                        dictionaryMovie.TrailerExists = movie.TrailerExists;
                        await _hubContext.Clients.All.SendAsync("getAllMoviesInfo", dictionaryMovie);
                    }
                    else
                    {
                        taskList.Add(GetMovieInfoAsync(movie));
                    }
                }
            }

            if (taskList.Count > 0)
            {
                _ = await Task.WhenAll(taskList);
            }

            _movieDictionary.ToList().ForEach(mov =>
            {
                if (Directory.Exists(mov.Value.FilePath) == false)
                {
                    _ = _movieDictionary.TryRemove(mov.Value.FilePath, out _);
                }
            });

            await _hubContext.Clients.All.SendAsync("completedAllMoviesInfo", _movieDictionary.Count);

            return _movieDictionary.Select(x => x.Value).ToList();
        }

        private void GetMovieDirectories(string directoryPath)
        {
            try
            {
                _movieDirectories.Clear();

                if (Directory.GetFiles(directoryPath).Length == 0)
                {
                    string[] subDirectories = Directory.GetDirectories(directoryPath);
                    if (Directory.GetFiles(subDirectories.FirstOrDefault()).Length == 0)
                    {
                        foreach (string directory in subDirectories)
                        {
                            GetMovieDirectories(directory);
                        }
                    }
                    else
                    {
                        _movieDirectories.Add(directoryPath);
                    }
                }
                else
                {
                    _movieDirectories.Add(directoryPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetMovieDirectories()");
            }
        }

        private Movie GetMovieFromDirectory(string movieDirectory)
        {
            if (Directory.GetFiles(movieDirectory).Length == 0)
            {
                return null;
            }

            bool trailerExists = Directory.GetFiles(movieDirectory).Where(name => name.Contains("-trailer")).Count() > 0;
            string filePath = Directory.GetFiles(movieDirectory).FirstOrDefault(file => !_excludedFileExtensions.Any(x => file.EndsWith(x)) && !file.Contains("-trailer"));
            string title = Regex.Replace(Path.GetFileNameWithoutExtension(filePath), @"\(.*", string.Empty).Trim().Replace("-trailer", string.Empty);
            string year = Regex.Match(Path.GetFileNameWithoutExtension(filePath), @"\(\d*").Captures.FirstOrDefault()?.Value.Replace("(", string.Empty);

            return new Movie
            {
                TrailerExists = trailerExists,
                FilePath = Path.GetDirectoryName(filePath),
                Title = title,
                Year = year
            };
        }

        public async void DownloadAllTrailers(IEnumerable<Movie> movieList)
        {
            foreach (Movie movie in movieList.OrderBy(movie => movie.Title))
            {
                if (movie.TrailerExists == false && string.IsNullOrEmpty(movie.TrailerURL) == false)
                {
                    if (DownloadTrailerAsync(movie).Result)
                    {
                        movie.TrailerExists = true;
                        await _hubContext.Clients.All.SendAsync("downloadAllTrailers", movie);
                    }
                }
            }

            await _hubContext.Clients.All.SendAsync("doneDownloadingAllTrailersListener", true);
        }

        public bool DeleteAllTrailers(IEnumerable<Movie> movieList)
        {
            ParallelLoopResult result = Parallel.ForEach(movieList, async movie =>
            {
                if (movie.TrailerExists)
                {
                    string filePath = Directory.GetFiles(movie.FilePath).Where(name => name.Contains("-trailer")).FirstOrDefault();
                    File.Delete(filePath);
                    movie.TrailerExists = false;
                    _movieDictionary.FirstOrDefault(mov => mov.Value.FilePath == movie.FilePath).Value.TrailerExists = false;
                    await _hubContext.Clients.All.SendAsync("deleteAllTrailers", movie);
                }
            });

            return result.IsCompleted;
        }

        private async Task<bool> DownloadTrailerAsync(Movie movie)
        {
            try
            {
                YoutubeClient youtube = new YoutubeClient();
                StreamManifest streamManifest = await youtube.Videos.Streams.GetManifestAsync(movie.TrailerURL);

                // Get highest quality muxed stream
                IVideoStreamInfo streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();

                if (streamInfo != null)
                {
                    // Download the stream to file
                    await youtube.Videos.Streams.DownloadAsync(streamInfo, Path.Combine(movie.FilePath, $"{movie.Title} ({movie.Year})-trailer.{streamInfo.Container}"));
                    _logger.LogInformation($"Successfully downloaded trailer for {movie.Title}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error downloading trailer for {movie.Title}\n{ex.Message}");
                await _hubContext.Clients.All.SendAsync("downloadAllTrailers", movie);
                return false;
            }
        }

        private async Task<Movie> GetMovieInfoAsync(Movie movie)
        {
            try
            {
                string uri = $"https://api.themoviedb.org/3/search/movie?language=en-US&query={movie.Title}&year={movie.Year}&api_key={_apiKey}";
                var res = await uri.GetJsonAsync<TMDBInfoResDto>();
                
                // JToken results = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync()).GetValue("results");
                // JToken singleResult = results.FirstOrDefault(j => j.Value<string>("title") == movie.Title);
                var singleResult = res.Movies.FirstOrDefault(x => x.Title == movie.Title);

                if (singleResult != null)
                {
                    movie.PosterPath = $"https://image.tmdb.org/t/p/w500/{singleResult.PosterPath}";
                    movie.Id = singleResult.Id;
                }
                else
                {
                    movie.PosterPath = $"https://image.tmdb.org/t/p/w500/{res.Movies.FirstOrDefault()?.PosterPath}";
                    movie.Id = res.Movies.FirstOrDefault()?.Id;
                }

                movie.TrailerURL = await GetTrailerURL(movie.Id);
                await _hubContext.Clients.All.SendAsync("getAllMoviesInfo", movie);

                movie = _movieDictionary.GetOrAdd(movie.FilePath, movie);
                return movie;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting movie info for {movie.Title}\n{ex.Message}");
                return null;
            }
        }

        private async Task<string> GetTrailerURL(int? id)
        {
            if (id == null) return string.Empty;
            
            string uri = $"https://api.themoviedb.org/3/movie/{id}/videos?api_key={_apiKey}&language={_trailerLanguage}";
            var res = await uri.GetJsonAsync<TMDBTrailerResDto>();

            foreach (var trailer in res.Trailers)
            {
                if (trailer.Site == "YouTube" && trailer.Type == "Trailer")
                    return trailer.Key;
            }
            
            return string.Empty;
        }
    }
}
