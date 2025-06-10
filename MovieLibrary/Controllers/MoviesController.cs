using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieLibrary.Data;
using MovieLibrary.Models;
using MovieLibrary.Services;
using System.Data;

namespace MovieLibrary.Controllers
{
    public class MoviesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly TmdbService _tmdbService;

        public MoviesController(AppDbContext context, TmdbService tmdbService)
        {
            _context = context;
            _tmdbService = tmdbService;
        }

        public async Task<IActionResult> TmdbSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return View(new List<TmdbMovie>());

            var result = await _tmdbService.SearchMovieAsync(query);
            var movies = result?.Results ?? new List<TmdbMovie>();

            return View(movies);
        }

        //Movie list
        public IActionResult Index()
        {
            var movies = _context.Movies.ToList();
            return View(movies);
        }

        //GET: Create
        public IActionResult Create()
        {
            return View("CreateEdit", new Movie());
        }

        //GET: Edit
        public IActionResult Edit(int id)
        {
            var movie = _context.Movies.Find(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View("CreateEdit",movie);
        }

        //POST: CreateEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEdit(Movie movie)
        {
            if (ModelState.IsValid)
            {
                //Empty input handeling
                if (string.IsNullOrWhiteSpace(movie.Description))
                {
                    movie.Description = "Bez popisu";
                }
                if (movie.Year == null)
                {
                    movie.Year = DateTime.Now.Year;
                }

                if (movie.Id == 0)
                {
                    _context.Add(movie);
                }
                else
                {
                    _context.Update(movie);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("CreateEdit", movie);
        }

        //POST: Movies/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
