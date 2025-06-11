using System.ComponentModel.DataAnnotations;

namespace MovieLibrary.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Název je povinný.")]
        public string Title { get; set; }
        public string? Description { get; set; }
        public int? Year { get; set; }
        public string? PosterUrl { get; set; }
        
        public ICollection<Actor> Actors { get; set; } = new List<Actor>();
        public ICollection<Director> Directors { get; set; } = new List<Director>();

        public int TmdbId { get; set; }
    }
}
