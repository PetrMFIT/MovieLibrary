using System.ComponentModel.DataAnnotations;

namespace MovieLibrary.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }
        public string? Description { get; set; }

        [Display(Name = "Release Year")]
        public int Year { get; set; }
        public string? PosterUrl { get; set; }
        public string? Director { get; set; }

        public Movie()
        {
            Description = "Bez popisu";
        }
    }
}
