using MovieLibrary.Controllers;
using MovieLibrary.Data;
using MovieLibrary.Models;
using MovieLibrary.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Moq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Runtime.InteropServices.Marshalling;

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


    /*** Movie Tests ***/

    // Add Movie
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

    // Edit Movie
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

    // Delete Movie
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

    // Delete Movie and orphaned actor
    [Fact]
    public async Task DeleteMovieAndActor()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = GetDbContext(dbName);
        var movie = new Movie { Title = "Test Movie", Year = 2025 };
        var person = new Person { Name = "Test Actor"};

        context.Movies.Add(movie);
        context.Persons.Add(person);
        var actor = new Actor {  MovieId = movie.Id, PersonId = person.Id };
        context.Actors.Add(actor);
        await context.SaveChangesAsync();

        var controller = new MoviesController(context, null);

        var result = await controller.Delete(movie.Id);

        var verificationContext = GetDbContext(dbName);
        var deletedActor = await verificationContext.Persons.FirstOrDefaultAsync(a => a.Name == "Test Actor");

        Assert.Null(deletedActor);
        Assert.IsType<RedirectToActionResult>(result);
    }

    // Delete Movie if actors are in another movie
    [Fact]
    public async Task DeleteMovieAndNotActor()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = GetDbContext(dbName);
        var movie1 = new Movie { Title = "Test Movie1", Year = 2024 };
        var movie2 = new Movie { Title = "Test Movie2", Year = 2025 };
        var person = new Person { Name = "Test Actor" };

        context.Movies.AddRange(movie1, movie2);
        context.Persons.Add(person);
        var actor1 = new Actor { MovieId = movie1.Id, PersonId = person.Id };
        var actor2 = new Actor { MovieId = movie2.Id, PersonId = person.Id };
        context.Actors.AddRange(actor1, actor2);
        await context.SaveChangesAsync();

        var controller = new MoviesController(context, null);

        var result = await controller.Delete(movie1.Id);

        var verificationContext = GetDbContext(dbName);
        var deletedActor = await verificationContext.Persons.FirstOrDefaultAsync(a => a.Name == "Test Actor");

        Assert.NotNull(deletedActor);
        Assert.IsType<RedirectToActionResult>(result);
    }


    /*** Person Tests ***/

    // Add Actor to db
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

    // Add Actor to movie
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

    // Add Director to db
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

    // Add Director to movie
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


    /*** Tmdb Service Tests ***/

    // Search movie
    [Fact]
    public async Task SearchMovieResultExist()
    {
        var mockTmdb = new Mock<ITmdbService>();
        var movie = new TmdbMovie
        {
            Id = 123,
            Title = "Test Movie",
            OriginalTitle = "Test Movie Original",
            Overview = "Description"
        };

        var searchResult = new TmdbSearchResult
        {
            Results = new List<TmdbMovie> { movie },
            Page = 1,
            TotalResults = 1,
            TotalPages = 1
        };

        mockTmdb.Setup(s => s.SearchMovieAsync("test")).ReturnsAsync(searchResult);

        var controller = new MoviesController(null, mockTmdb.Object);

        var result = await controller.TmdbSearch("test") as JsonResult;

        Assert.NotNull(result);
        var data = Assert.IsType<List<MovieDto>>(result.Value);
        Assert.Single(data);

        var movieDto = data[0];

        Assert.Equal(123, movieDto.Id);
        Assert.Equal(movieDto.Title, "Test Movie");
        Assert.Equal(movieDto.OriginalTitle, "Test Movie Original");
        Assert.Equal(movieDto.Description, "Description");
    }
}

