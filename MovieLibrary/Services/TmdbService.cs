using Microsoft.Extensions.Options;
using MovieLibrary.Models;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MovieLibrary.Services
{
    public class TmdbService : ITmdbService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public TmdbService(HttpClient httpClient, IOptions<TmdbSettings> options)
        {
            _httpClient = httpClient;
            _apiKey = options.Value.ApiKey;
        }

        public async Task<TmdbSearchResult?> SearchMovieAsync(string query)
        {
            var url = $"https://api.themoviedb.org/3/search/movie?api_key={_apiKey}&query={query}&language=cs-CZ";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize <TmdbSearchResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result;
        }

        public async Task<TmdbMovieDto?> GetMovieWithCreditsAsync(int tmdbId)
        {
            var url = $"https://api.themoviedb.org/3/movie/{tmdbId}?api_key={_apiKey}&language=cs-CZ&append_to_response=credits";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TmdbMovie>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (result == null) return null;

            return new TmdbMovieDto
            {
                Title = result.Title,
                OriginalTitle = result.OriginalTitle,
                Overview = result.Overview,
                ReleaseDate = result.ReleaseDate?.Length >= 4 ? result.ReleaseDate.Substring(0,4) : "",
                PosterPath = string.IsNullOrEmpty(result.PosterPath) ? null : $"https://image.tmdb.org/t/p/w342{result.PosterPath}",
                BackgroundPath = string.IsNullOrEmpty(result.BackgroundPath) ? null : $"https://image.tmdb.org/t/p/w342{result.BackgroundPath}",
                Actors = result.Credits?.Cast?.Take(9).Select(a => new PersonDto
                {
                    Name = a.Name,
                    PhotoUrl = string.IsNullOrEmpty(a.ProfilePath) ? null : $"https://image.tmdb.org/t/p/w185{a.ProfilePath}"
                }).ToList() ?? new List<PersonDto>(),
                Directors = result.Credits?.Crew?.Where(c => c.Job == "Director").Select(d => new PersonDto
                {
                    Name = d.Name,
                    PhotoUrl = string.IsNullOrEmpty(d.ProfilePath) ? null : $"https://image.tmdb.org/t/p/w185{d.ProfilePath}"
                }).ToList() ?? new List<PersonDto>()

            };
        }

        /*public async Task<TmdbMovie?> GetMovieDetailsAsync(int tmdbId)
        {
            var url = $"https://api.themoviedb.org/3/movie/{tmdbId}?api_key={_apiKey}&language=cs-CZ";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TmdbMovie>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result;
        }*/
    }

    public class TmdbSearchResult
    {
        public int Page {  get; set; }
        public List<TmdbMovie> Results { get; set; }
        public int TotalResults { get; set; }
        public int TotalPages { get; set; } 

    }

    public class TmdbCredits
    {
        [JsonPropertyName("cast")]
        public List<TmdbCast>? Cast { get; set; }

        [JsonPropertyName("crew")]
        public List<TmdbCrew>? Crew { get; set; }
    }

    public class TmdbCast
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("profile_path")]
        public string? ProfilePath { get; set; }
    }

    public class TmdbCrew
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("job")]
        public string Job { get; set; } = "";

        [JsonPropertyName("profile_path")]
        public string? ProfilePath { get; set; }
    }

    public class TmdbMovie
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("poster_path")]
        public string PosterPath { get; set; }

        [JsonPropertyName("release_date")]
        public string ReleaseDate { get; set; }

        [JsonPropertyName("overview")]
        public string Overview { get; set; }

        [JsonPropertyName("original_title")]
        public string OriginalTitle { get; set; }

        [JsonPropertyName("backdrop_path")]
        public string BackgroundPath { get; set; }

        [JsonPropertyName("credits")]
        public TmdbCredits? Credits { get; set; }
    }

    public class TmdbSettings
    {
        public string ApiKey { get; set; }
    }
}
