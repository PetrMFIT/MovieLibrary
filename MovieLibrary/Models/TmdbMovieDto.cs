namespace MovieLibrary.Models;
public class TmdbMovieDto
{
	public int Id { get; set; }
	public string Title { get; set; } = "";
	public string OriginalTitle { get; set; } = "";
	public string Overview { get; set; } = "";
	public string ReleaseDate { get; set; } = "";
	public string? PosterPath {  get; set; }
	public string? BackgroundPath { get; set; }

	public List<PersonDto> Actors { get; set; } = new();
	public List<PersonDto> Directors { get; set; } = new();
}
