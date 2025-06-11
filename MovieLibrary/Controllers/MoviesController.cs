using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieLibrary.Data;
using MovieLibrary.Migrations;
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

        [HttpGet]
        public async Task<IActionResult> TmdbSearch(string term)
        {
            var searchResult = await _tmdbService.SearchMovieAsync(term);
            if (searchResult == null || !searchResult.Results.Any())
                return Json(new List<object>());

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

        [HttpGet]
        public async Task<IActionResult> TmdbMovieDetails(int tmdbId)
        {
            if (tmdbId <= 0)
                return BadRequest("Neplatné TMDb ID.");

            var details = await _tmdbService.GetMovieDetailsAsync(tmdbId);
            if (details == null)
            {
                System.Diagnostics.Debug.WriteLine("HERE3");
                return Json(null);
            }
               
            var result = new
            {
                actors = details.Credits?.Cast?.Take(9).Select(a => new PersonDto
                {
                    Name = a.Name,
                    PhotoUrl = string.IsNullOrEmpty(a.ProfilePath) ? null : $"https://image.tmdb.org/t/p/w185{a.ProfilePath}"
                }).ToList() ?? new List<PersonDto>(),
                directors = details.Credits?.Crew?.Where(c => c.Job == "Director").Select(d => new PersonDto
                {
                    Name = d.Name,
                    PhotoUrl = string.IsNullOrEmpty(d.ProfilePath) ? null : $"https://image.tmdb.org/t/p/w185{d.ProfilePath}"
                }).ToList() ?? new List<PersonDto>()
            };

            System.Diagnostics.Debug.WriteLine("HERE2: Actors Count = " + result.actors.Count);
            foreach (var actor in result.actors)
            {
                System.Diagnostics.Debug.WriteLine($"Actor: Name = {actor.Name}, PhotoUrl = {actor.PhotoUrl}");
            }
            return Json(result);
        }

        //Movie list
        public IActionResult Index()
        {
            var movies = _context.Movies.ToList();
            return View(movies);
        }

        //GET: Create
        public async Task<IActionResult> Create()
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
