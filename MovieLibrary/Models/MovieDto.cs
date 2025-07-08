namespace MovieLibrary.Models;

public class MovieDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string OriginalTitle { get; set; } = "";
    public string Description { get; set; } = "";
    public string Year { get; set; } = "";
    public string Poster { get; set; } = "";
    public string Background { get; set; } = "";
}
