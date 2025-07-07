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
    private AppDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;


        return new AppDbContext(options);
    }

    // Movie test
    [Fact]
    public async Task InsertMovieIntoDatabase()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = GetDbContext(dbName);
        var controller = new MoviesController(context, null);

        var movie = new Movie { Title = "Test Movie", Year = 2025 };

        var result = await controller.CreateEdit(movie, 0, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());

        var verificationContext = GetDbContext(dbName);
        var insertedMovie = await verificationContext.Movies.FirstOrDefaultAsync(m => m.Title == "Test Movie");

        Assert.NotNull(insertedMovie);
        Assert.Equal(2025, insertedMovie.Year);
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task EditMovieDatabase()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = GetDbContext(dbName);
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

        var verificationContext = GetDbContext(dbName);
        var movieInDb = await verificationContext.Movies.FindAsync(movie.Id);

        Assert.Equal("New Title", movieInDb.Title);
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task DeleteMovieFromDatabase()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = GetDbContext(dbName);
        var movie = new Movie { Title = "Test Movie", Year = 2025 };
        context.Movies.Add(movie);
        await context.SaveChangesAsync();

        var controller = new MoviesController(context, null);

        var result = await controller.Delete(movie.Id);

        var verificationContext = GetDbContext(dbName);
        var deletedMovie = await verificationContext.Movies.FindAsync(movie.Id);

        Assert.Null(deletedMovie);
        Assert.IsType<RedirectToActionResult>(result);
    }

    // Person tests
    [Fact]
    public async Task InsertActorIntoDatabase()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = GetDbContext(dbName);
        var controller = new MoviesController(context, null);
        var movie = new Movie { Title = "Test Movie", Year = 2025 };
        string[] actors = new[] { "Test Name" };

        var result = await controller.CreateEdit(movie, 0, actors, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());

        var verificationContext = GetDbContext(dbName);
        var insertedPerson = await verificationContext.Persons.FirstOrDefaultAsync(p => p.Name == "Test Name");

        Assert.NotNull(insertedPerson);
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task InsertActorIntoMovie()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = GetDbContext(dbName);
        var controller = new MoviesController(context, null);
        var movie = new Movie { Title = "Test Movie", Year = 2025 };
        string[] actors = new[] { "Test Name" };

        var result = await controller.CreateEdit(movie, 0, actors, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());

        var verificationContext = GetDbContext(dbName);
        var insertedPerson = await verificationContext.Persons.FirstOrDefaultAsync(p => p.Name == "Test Name");

        var insertedMovie = await verificationContext.Movies.Include(m => m.Actors).FirstOrDefaultAsync();

        Assert.Contains(insertedMovie.Actors, a => a.PersonId == insertedPerson.Id);
        Assert.NotNull(insertedPerson);
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task InsertDIrectorIntoDatabase()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = GetDbContext(dbName);
        var controller = new MoviesController(context, null);
        var movie = new Movie { Title = "Test Movie", Year = 2025 };
        string[] directors = new[] { "Test Name" };

        var result = await controller.CreateEdit(movie, 0, Array.Empty<string>(), Array.Empty<string>(), directors, Array.Empty<string>());

        var verificationContext = GetDbContext(dbName);
        var insertedPerson = await verificationContext.Persons.FirstOrDefaultAsync(p => p.Name == "Test Name");

        Assert.NotNull(insertedPerson);
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task InsertDirectorIntoMovie()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = GetDbContext(dbName);
        var controller = new MoviesController(context, null);
        var movie = new Movie { Title = "Test Movie", Year = 2025 };
        string[] directors = new[] { "Test Name" };

        var result = await controller.CreateEdit(movie, 0, Array.Empty<string>(), Array.Empty<string>(), directors, Array.Empty<string>());

        var verificationContext = GetDbContext(dbName);
        var insertedPerson = await verificationContext.Persons.FirstOrDefaultAsync(p => p.Name == "Test Name");

        var insertedMovie = await verificationContext.Movies.Include(m => m.Directors).FirstOrDefaultAsync();

        Assert.Contains(insertedMovie.Directors, d => d.PersonId == insertedPerson.Id);
        Assert.NotNull(insertedPerson);
        Assert.IsType<RedirectToActionResult>(result);
    }
}
