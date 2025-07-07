using MovieLibrary.Controllers;
using MovieLibrary.Data;
using MovieLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace MovieLibrary.Tests;

public class PersonsControllerTests
{
    private AppDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new AppDbContext(options);
    }

    // Delete Person
    [Fact]
    public async Task DeletePersonFromDatabase()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = GetDbContext(dbName);
        var person = new Person { Name = "Test Person" };
        context.Persons.Add(person);
        await context.SaveChangesAsync();

        var controller = new PersonsController(context);

        var result = await controller.Delete(person.Id);

        var verificationContext = GetDbContext(dbName);
        var deletedPerson = await verificationContext.Persons.FindAsync(person.Id);

        Assert.Null(deletedPerson);
        Assert.IsType<RedirectToActionResult>(result);
    }
}
