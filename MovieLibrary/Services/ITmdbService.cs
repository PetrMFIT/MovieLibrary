using System.Threading.Tasks;
using MovieLibrary.Models;

namespace MovieLibrary.Services;

public interface ITmdbService
{
	Task<TmdbSearchResult?> SearchMovieAsync(string query);
	Task<TmdbMovie?> GetMovieCreditsAsync(int tmdb);
	Task<TmdbMovie?> GetMovieDetailsAsync(int tmdbId);
}
