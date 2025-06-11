using System.ComponentModel.DataAnnotations;

namespace MovieLibrary.Models
{
    public class Person
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Jméno je povinné.")]
        public string Name { get; set; } = "";
        public string? PhotoUrl { get; set; }

        public ICollection<Actor> ActingMovies { get; set; } = new List<Actor>();
        public ICollection<Director> DirectingMovies { get; set; } = new List<Director>();
    }
}
