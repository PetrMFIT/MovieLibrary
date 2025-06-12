using MovieLibrary.Controllers;
using MovieLibrary.Data;
using MovieLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace MovieLibrary.Tests;

public class MoviesControllerTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;


        return new AppDbContext(options);
    }

    [Fact]
    public async Task InsertMovieIntoDatabase()
    {
        var context = GetDbContext();
        var controller = new MoviesController(context, null);

        var movie = new Movie { Title = "Test Movie", Year = 2025 };

        var result = await controller.CreateEdit(movie, 0, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());

        var insertedMovie = await context.Movies.FirstOrDefaultAsync(m => m.Title == "Test Movie");

        Assert.NotNull(insertedMovie);
        Assert.Equal(2025, insertedMovie.Year);
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task EditMovieDatabase()
    {
        var context = GetDbContext();
        var movie = new Movie { Title = "Test Movie", Year = 2025 };
        context.Movies.Add(movie);
        await context.SaveChangesAsync();

        var controller = new MoviesController(context, null);

        var editMovie = new Movie
        {
            Id = movie.Id,
            Title = "New Title",
            Year = movie.Year
        };

        var result = await controller.CreateEdit(editMovie, 0, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());

        var movieInDb = await context.Movies.FindAsync(movie.Id);
        Assert.Equal("New Title", movieInDb.Title);
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task DeleteMovieFromDatabase()
    {
        var context = GetDbContext();
        var movie = new Movie { Title = "Test Movie", Year = 2025 };
        context.Movies.Add(movie);
        await context.SaveChangesAsync();

        var controller = new MoviesController(context, null);

        var result = await controller.Delete(movie.Id);

        //Assert
        var deletedMovie = await context.Movies.FindAsync(movie.Id);
        Assert.Null(deletedMovie);
        Assert.IsType<RedirectToActionResult>(result);
    }
}
