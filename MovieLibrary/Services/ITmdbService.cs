using System.Threading.Tasks;
using MovieLibrary.Models;

namespace MovieLibrary.Services;

public interface ITmdbService
{
	Task<TmdbSearchResult?> SearchMovieAsync(string query);
	Task<TmdbMovieDto?> GetMovieWithCreditsAsync(int tmdb);
}
