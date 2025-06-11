namespace MovieLibrary.Models
{
    public class Actor
    {
        public int PersonId { get; set; }
        public Person Person { get; set; } = null;

        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null;
    }
}
