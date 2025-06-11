using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieLibrary.Data;
using MovieLibrary.Migrations;
using MovieLibrary.Models;
using MovieLibrary.Services;
using System;
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

        // Searching through TheMovieDatabase
        [HttpGet]
        public async Task<IActionResult> TmdbSearch(string term)
        {
            var searchResult = await _tmdbService.SearchMovieAsync(term);
            if (searchResult == null || !searchResult.Results.Any())
                return Json(new List<object>());

            // Saving main informations about movie
            var movies = searchResult.Results.Select(movie => new {
                id = movie.Id,
                title = movie.Title,
                originalTitle = movie.OriginalTitle,
                description = movie.Overview,
                year = string.IsNullOrEmpty(movie.ReleaseDate) ? "" : movie.ReleaseDate.Substring(0, 4),
                poster = string.IsNullOrEmpty(movie.PosterPath) ? "" : $"https://image.tmdb.org/t/p/w342{movie.PosterPath}",
                background = string.IsNullOrEmpty(movie.BackgroundPath) ? "" : $"https://image.tmdb.org/t/p/w342{movie.BackgroundPath}"
            }).ToList();

            return Json(movies);
        }

        // Fetching movie detailas (cast, crew)
        [HttpGet]
        public async Task<IActionResult> TmdbMovieDetails(int tmdbId)
        {
            if (tmdbId <= 0)
                return BadRequest("Neplatné TMDb ID.");

            var credits = await _tmdbService.GetMovieCreditsAsync(tmdbId);

            if (credits == null)
                return Json(null);

            var details = await _tmdbService.GetMovieDetailsAsync(tmdbId);
            if (details == null)
                return Json(null);
               
            var result = new
            {
                actors = credits.Credits?.Cast?.Take(9).Select(a => new PersonDto
                {
                    Name = a.Name,
                    PhotoUrl = string.IsNullOrEmpty(a.ProfilePath) ? null : $"https://image.tmdb.org/t/p/w185{a.ProfilePath}"
                }).ToList() ?? new List<PersonDto>(),
                directors = credits.Credits?.Crew?.Where(c => c.Job == "Director").Select(d => new PersonDto
                {
                    Name = d.Name,
                    PhotoUrl = string.IsNullOrEmpty(d.ProfilePath) ? null : $"https://image.tmdb.org/t/p/w185{d.ProfilePath}"
                }).ToList() ?? new List<PersonDto>()
            };

            return Json(result);
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
            var movie = _context.Movies
                .Include(m => m.Actors).ThenInclude(ma => ma.Person)
                .Include(m => m.Directors).ThenInclude(md => md.Person)
                .FirstOrDefault(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }
            return View("CreateEdit", movie);
        }

        //POST: CreateEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEdit(Movie movie, int TmdbId, string[] actors, string[] actorPhotos, string[] directors, string[] directorPhotos)
        {
            if (ModelState.IsValid)
            {
                // Handle empty inputs
                if (string.IsNullOrWhiteSpace(movie.Description))
                    movie.Description = "Bez popisu";
                if (movie.Year == null)
                    movie.Year = DateTime.Now.Year;

                // Process actors
                var actorEntities = new List<Actor>();
                for (int i = 0; i < actors.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(actors[i]))
                        continue;

                    var person = _context.Persons.FirstOrDefault(p => p.Name == actors[i]);
                    if (person == null)
                    {
                        person = new Person
                        {
                            Name = actors[i],
                            PhotoUrl = actorPhotos.Length > i ? actorPhotos[i] : null
                        };
                        _context.Persons.Add(person);
                        await _context.SaveChangesAsync();
                    }
                    actorEntities.Add(new Actor { PersonId = person.Id });
                }

                // Process directors
                var directorEntities = new List<Director>();
                for (int i = 0; i < directors.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(directors[i]))
                        continue;

                    var person = _context.Persons.FirstOrDefault(p => p.Name == directors[i]);
                    if (person == null)
                    {
                        person = new Person
                        {
                            Name = directors[i],
                            PhotoUrl = directorPhotos.Length > i ? directorPhotos[i] : null
                        };
                        _context.Persons.Add(person);
                        await _context.SaveChangesAsync();
                    }
                    directorEntities.Add(new Director { PersonId = person.Id });
                }

                movie.TmdbId = TmdbId;

                if (movie.Id == 0)
                {
                    if (TmdbId > 0)
                    {
                        var existing = await _context.Movies.FirstOrDefaultAsync(m => m.TmdbId == TmdbId);
                        if (existing != null)
                        {
                            //return RedirectToAction("Edit", new { id = existing.Id });
                            return RedirectToAction("Index");
                        }
                    }

                    // Create new movie
                    movie.Actors = actorEntities;
                    movie.Directors = directorEntities;
                    _context.Add(movie);
                }
                else
                {
                    // Update existing movie
                    var existingMovie = _context.Movies
                        .Include(m => m.Actors)
                        .Include(m => m.Directors)
                        .FirstOrDefault(m => m.Id == movie.Id);

                    if (existingMovie == null)
                        return NotFound();

                    // Update scalar properties
                    _context.Entry(existingMovie).CurrentValues.SetValues(movie);

                    // Update actors
                    existingMovie.Actors.Clear();
                    foreach (var actor in actorEntities)
                    {
                        existingMovie.Actors.Add(new Actor { MovieId = movie.Id, PersonId = actor.PersonId });
                    }

                    // Update directors
                    existingMovie.Directors.Clear();
                    foreach (var director in directorEntities)
                    {
                        existingMovie.Directors.Add(new Director { MovieId = movie.Id, PersonId = director.PersonId });
                    }

                    _context.Update(existingMovie);
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
            var movie = await _context.Movies
                .Include(m => m.Actors)
                .Include(m => m.Directors)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
                return RedirectToAction(nameof(Index));

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            var allPersonids = movie.Actors.Select(a => a.PersonId)
                .Concat(movie.Directors.Select(d => d.PersonId))
                .Distinct();

            foreach (var personId in allPersonids)
            {
                var isStillUsed = await _context.Actors.AnyAsync(a => a.PersonId == personId) 
                                || await _context.Directors.AnyAsync(d =>  d.PersonId == personId);

                if (!isStillUsed)
                {
                    var person = await _context.Persons.FindAsync(personId);
                    if (person != null)
                    {
                        _context.Persons.Remove(person);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
