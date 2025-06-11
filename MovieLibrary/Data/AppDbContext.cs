using Microsoft.EntityFrameworkCore;
using MovieLibrary.Models;

namespace MovieLibrary.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Director> Directors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Actor>()
                .HasKey(a => new { a.MovieId, a.PersonId });

            modelBuilder.Entity<Actor>()
                .HasOne(a => a.Movie)
                .WithMany(m => m.Actors)
                .HasForeignKey(a => a.MovieId);

            modelBuilder.Entity<Actor>()
                .HasOne(a => a.Person)
                .WithMany(p => p.ActingMovies)
                .HasForeignKey(a => a.PersonId);


            modelBuilder.Entity<Director>()
                .HasKey(d => new { d.MovieId, d.PersonId });

            modelBuilder.Entity<Director>()
                .HasOne(d => d.Movie)
                .WithMany(m => m.Directors)
                .HasForeignKey(d => d.MovieId);

            modelBuilder.Entity<Director>()
                .HasOne(d => d.Person)
                .WithMany(p => p.DirectingMovies)
                .HasForeignKey(d => d.PersonId);
        }
    }
}
