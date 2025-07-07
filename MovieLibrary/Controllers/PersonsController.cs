using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieLibrary.Data;
using MovieLibrary.Models;

namespace MovieLibrary.Controllers
{
    public class PersonsController : Controller
    {
        private readonly AppDbContext _context;

        public PersonsController(AppDbContext context)
        {
            _context = context;
        }

        //Person list
        public IActionResult PersonList()
        {
            var persons = _context.Persons
                .Include(p => p.ActingMovies)
                .ThenInclude(a => a.Movie)
                .Include(p => p.DirectingMovies)
                .ThenInclude(d => d.Movie)
                .ToList();
            return View(persons);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int personId)
        {
            var person = await _context.Persons
                .Include(p => p.ActingMovies)
                .Include(p => p.DirectingMovies)
                .FirstOrDefaultAsync(p => p.Id == personId);

            if (person == null)
                return NotFound();

            if (!person.ActingMovies.Any() && !person.DirectingMovies.Any())
            {
                _context.Persons.Remove(person);
                await _context.SaveChangesAsync();
            }
            else
            {
                TempData["Error"] = "Nelze smazat osobu, protože je piřazena k jednomu nebo více filmům.";
            }
            return RedirectToAction("PersonList");
        }
    }
}