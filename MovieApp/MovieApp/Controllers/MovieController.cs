using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using System.Web;
using System.Web.Mvc;
using MovieApp.Context;
using MovieApp.Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity;

namespace MovieApp.Controllers
{
    public class MovieController : Controller
    {
        MovieContext db = new MovieContext();

        public ActionResult Index()
        {
            return View(db.Genres.ToList());
        }

        public ActionResult MovieCheck(string title, string year, string id)
        {
            if (id == "undefined")
                return Json(!db.Movies.Any(m => m.Title == title && m.Year.ToString() == year), JsonRequestBehavior.AllowGet);
            return Json(!db.Movies.Any(m => m.Title == title && m.Year.ToString() == year && m.Id.ToString() != id), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMovie(int id) // get information for movie given id
        {
            Movie movie = db.Movies.First(m => m.Id == id);
            return PartialView(movie);
        }

        public JsonResult Search(string search) //for autocomplete
        {
            //return list of movies with substrings that match the search terms
            var data = db.Movies.Where(m => m.Title.ToLower().Contains(search.ToLower())).Select(
                m => new MovieViewModel {
                    MovieID = m.Id,
                    Title = m.Title,
                    Year = m.Year }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Manage()
        {            
            return View(db.Movies.ToList());
        }

        [Authorize]
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.Genres = db.Genres.Select(
                g => new AssignedGenre {
                    id = g.Id,
                    Name = g.Name,
                    Assigned = false
                }).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult Create(Movie movie, string[] SelectedGenres)
        {
            if (ModelState.IsValid) //check if movie already exists
            {
                movie.Genres = new List<Genre>();
                if (SelectedGenres != null)
                    movie.Genres = db.Genres.Where(g => SelectedGenres.Contains(g.Id.ToString())).ToList();
                db.Movies.Add(movie);
                db.SaveChanges();
                TempData["Success"] = String.Format("{0} ({1}) created", movie.Title, movie.Year);
            }
            return RedirectToAction("Manage");
        }

        [HttpGet]
        [Authorize]
        public ActionResult Edit(int id)
        {
            Movie movie = db.Movies.First(m => m.Id == id);
            PopulateGenres(movie);
            return View(movie);
        }

        public ActionResult Edit(Movie movie, string[] SelectedGenres)
        {
            if (ModelState.IsValid)
            {
                Movie oldmovie = db.Movies.First(m => movie.Id == m.Id); //get old movie information

                //update movie in database 
                oldmovie.Title = movie.Title;
                oldmovie.Year = movie.Year;
                UpdateMovieGenres(oldmovie, SelectedGenres); //add selected genres to movie's genres
                db.Entry(oldmovie).State = EntityState.Modified;
                db.SaveChanges(); 
                TempData["Success"] = String.Format("{0} ({1}) updated", movie.Title, movie.Year);
            }
            RouteData.Values.Remove("id"); //remove route value from url
            return RedirectToAction("Manage");
        }

        public ActionResult Delete(int id)
        {
            Movie movie = db.Movies.First(m => id == m.Id);
            db.Movies.Attach(movie);
            db.Movies.Remove(movie);
            db.SaveChanges();
            TempData["Success"] = String.Format("{0} ({1}) deleted", movie.Title, movie.Year);
            RouteData.Values.Remove("id"); //remove route value from url
            return Json(Url.Action("Manage", "Movie"), JsonRequestBehavior.AllowGet); //return to manage page via javascript
        }


        //Take a movie and use its list of genres to generate a 
        // list of selected genres
        private void PopulateGenres(Movie movie)
        {
            //go through all genres in database and record whether or not each one contains the movie
            ViewBag.Genres = db.Genres.Select(
                g => new AssignedGenre {
                    id = g.Id,
                    Name = g.Name,
                    Assigned = db.Movies.FirstOrDefault(m => m.Id == movie.Id).Genres.Any(genre => genre.Id == g.Id)
                }).ToList();
        }

        //takes all the selected genres as id strings and updats the movie's genres
        private void UpdateMovieGenres(Movie movie, string[] SelectedGenres)
        {
            //make a new, empty list if no movies were chosen
            if (SelectedGenres == null)
            {
                movie.Genres.Clear();
                return;
            }
            var selected = new HashSet<int>(SelectedGenres.Select(int.Parse).ToList());
            //add new selected movies and remove deselected ones
            movie.Genres = movie.Genres.Union(db.Genres.Where(g => selected.Contains(g.Id))).Except(db.Genres.Where(g => !selected.Contains(g.Id))).ToList();
        }

        [Authorize]
        public ActionResult ResetDatabase()
        {
            ClearData();
            AddTestData();
            return RedirectToAction("Index"); 
        }

        private void ClearData()//delete all rows in movies and genres
        {
            db.Genres.RemoveRange(db.Genres);
            db.Movies.RemoveRange(db.Movies);
            db.SaveChanges();
        }

        private void AddTestData()
        {
            List<string> genreNames = new List<string> { "Action", "Adventure", "Romance", "Comedy", "SciFi", "Horror", "Fantasy", "Sports" };
            foreach (var name in genreNames)
                db.Genres.Add(new Genre { Name = name });

            db.SaveChanges();

            db.Movies.Add(new Movie { Title = "Twilight", Year = 2008, Genres = new List<Genre> { db.Genres.First(g => g.Name == "Romance"), db.Genres.First(g=>g.Name == "Fantasy") } });
            db.Movies.Add(new Movie { Title = "The Dark Knight", Year = 2008, Genres = new List<Genre> { db.Genres.First(g => g.Name == "Action") } });
            db.Movies.Add(new Movie { Title = "The Dark Knight Rises", Year = 2012, Genres = new List<Genre> { db.Genres.First(g => g.Name == "Action") } });
            db.Movies.Add(new Movie { Title = "Kung Fu Hustle", Year = 2004, Genres = new List<Genre> { db.Genres.First(g => g.Name == "Action"), db.Genres.First(g => g.Name == "Comedy") } });
            db.Movies.Add(new Movie { Title = "The Ring", Year = 2002, Genres = new List<Genre> { db.Genres.First(g => g.Name == "Horror") } });
            db.Movies.Add(new Movie { Title = "The Grudge", Year = 2004, Genres = new List<Genre> { db.Genres.First(g => g.Name == "Horror") } });
            db.Movies.Add(new Movie { Title = "The Hangover", Year = 2009, Genres = new List<Genre> { db.Genres.First(g => g.Name == "Comedy") } });
            db.Movies.Add(new Movie { Title = "Star Trek", Year = 2009, Genres = new List<Genre> { db.Genres.First(g => g.Name == "SciFi"), db.Genres.First(g => g.Name == "Action"), db.Genres.First(g => g.Name == "Adventure") } });
            db.Movies.Add(new Movie { Title = "Pirates of the Carribean: The Curse of the Black Pearl", Year = 2003, Genres = new List<Genre> { db.Genres.First(g => g.Name == "Comedy"), db.Genres.First(g => g.Name == "Action"), db.Genres.First(g => g.Name == "Adventure"), db.Genres.First(g => g.Name == "Fantasy") } });
            db.Movies.Add(new Movie { Title = "The Blind Side", Year = 2009, Genres = new List<Genre> { db.Genres.First(g => g.Name == "Sports")} });
            db.Movies.Add(new Movie { Title = "Star Wars: Episode IV A New Hope", Year = 1977, Genres = new List<Genre> { db.Genres.First(g => g.Name == "SciFi"), db.Genres.First(g => g.Name == "Action"), db.Genres.First(g => g.Name == "Adventure") } });
            db.Movies.Add(new Movie { Title = "Saw", Year = 2004, Genres = new List<Genre> { db.Genres.First(g => g.Name == "Horror") }});
            db.Movies.Add(new Movie { Title = "It Follows", Year = 2014, Genres = new List<Genre> { db.Genres.First(g => g.Name == "Horror") } });
            db.Movies.Add(new Movie { Title = "The Cabin in the Woods", Year = 2011, Genres = new List<Genre> { db.Genres.First(g => g.Name == "Horror") } });
            db.Movies.Add(new Movie { Title = "Jaws", Year = 1975, Genres = new List<Genre> { db.Genres.First(g => g.Name == "Horror") } });
            db.Movies.Add(new Movie { Title = "Resident Evil", Year = 2014, Genres = new List<Genre> { db.Genres.First(g => g.Name == "Horror"), db.Genres.First(g => g.Name == "SciFi"), db.Genres.First(g => g.Name == "Action") } });
            db.Movies.Add(new Movie { Title = "Tropical Thunder", Year = 2008, Genres = new List<Genre> { db.Genres.First(g => g.Name == "Comedy") } });
            db.Movies.Add(new Movie { Title = "Monty Python and the Holy Grail", Year = 1975, Genres = new List<Genre> { db.Genres.First(g => g.Name == "Comedy"), db.Genres.First(g => g.Name == "Adventure") } });
            db.Movies.Add(new Movie { Title = "Pineapple Express", Year = 2008, Genres = new List<Genre> { db.Genres.First(g => g.Name == "Comedy") } });
            db.Movies.Add(new Movie { Title = "Red", Year = 2010, Genres = new List<Genre> { db.Genres.First(g => g.Name == "Comedy"), db.Genres.First(g => g.Name == "Action"), db.Genres.First(g => g.Name == "Comedy") } });
            db.SaveChanges();
        }
    }    
}

