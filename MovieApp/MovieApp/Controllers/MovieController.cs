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
            MovieDatabase data = new MovieDatabase(db.Movies.ToList(), db.Genres.ToList());
            return View(data);
        }

        [Authorize]
        public ActionResult Manage()
        {
            return View();
        }

        //*****************GENRES********************
        public ActionResult Genre_All(string name)
        {
            Genre genre = db.Genres.ToList().Find(g => g.Name == name);
            return View(genre);
        }
        public JsonResult AllGenres()
        {
            List<Genre> genre_list = db.Genres.ToList();
            List<GenreViewModel> genres = new List<GenreViewModel>();
            foreach (var g in genre_list)
                genres.Add(new GenreViewModel(g.Id, g.Name));
            JsonResult result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            result.Data = genres;
            return result;
        }

        [Authorize]
        public ActionResult GenreManage()
        {
            return View(db.Genres.ToList());
        }

        [Authorize]
        [HttpGet]
        public ActionResult GenreCreate()
        {
            var viewModel = new List<AssignedMovie>();
            foreach (var movie in db.Movies)
            {
                viewModel.Add(new AssignedMovie
                {
                    id = movie.Id,
                    Title = movie.Title,
                    Year = movie.Year,
                    Assigned = false
                });
            }
            ViewBag.Movies = viewModel;
            return PartialView();
        }

        [HttpPost]
        public ActionResult GenreCreate(Genre genre, string[] SelectedMovies)
        {            
            if (string.IsNullOrWhiteSpace(genre.Name))
            {
                TempData["Error"] = "Missing Name";
            }
            else
            {
                genre.Name = genre.Name.Trim();
                if (db.Genres.ToList().Find(g => g.Name.ToLower() == genre.Name.ToLower()) == null)
                {
                    //genre.Id = db.Genres.ToList().Max(g=>g.Id) + 1;

                    genre.Movies = new List<Movie>();
                    if(SelectedMovies != null)
                    {
                        foreach(var idstring in SelectedMovies)
                        {
                            int id = Convert.ToInt32(idstring);
                            genre.Movies.Add(db.Movies.ToList().Find(m => m.Id == id));
                        }
                    }

                    db.Genres.Add(genre);
                    db.SaveChanges();
                    TempData["Success"] = genre.Name + " created";
                }
                else
                {
                    TempData["Error"] = genre.Name + " already exists";
                }

            }
            return RedirectToAction("GenreManage");
        }

        [Authorize]
        [HttpGet]
        public ActionResult GenreEdit(int genreId)
        {
            Genre genre = db.Genres.ToList().Find(g => g.Id == genreId);
            PopulateMovies(genre);
            return PartialView(genre);
        }
       
        private void PopulateMovies(Genre genre)
        {
            var movies = db.Movies;
            var genremovies = new HashSet<int>(genre.Movies.Select(m => m.Id));
            var viewModel = new List<AssignedMovie>();
            foreach (var movie in movies)
            {
                viewModel.Add(new AssignedMovie
                {
                    id = movie.Id,
                    Title = movie.Title,
                    Year = movie.Year,
                    Assigned = genremovies.Contains(movie.Id)
                });
            }
            ViewBag.Movies = viewModel;
        }

        private void UpdateGenreMovies(Genre genre, string[] SelectedMovies)
        {
            if (SelectedMovies == null)
            {
                genre.Movies.Clear();
                return;
            }       

            var selected = new HashSet<string>(SelectedMovies);
            var genremovies = new HashSet<int>(genre.Movies.Select(m => m.Id));
            foreach (var movie in db.Movies)
            {
                if (selected.Contains(movie.Id.ToString()))
                {
                    if (!genremovies.Contains(movie.Id))
                    {
                        genre.Movies.Add(movie);
                    }
                }
                else
                {
                    if (genremovies.Contains(movie.Id))
                    {
                        genre.Movies.Remove(movie);
                    }
                }
            }
        }
        public ActionResult GenreEdit(Genre genre, string[] SelectedMovies)
        {           
            if (Request.Form["save"] != null)
            {                
                Genre oldgenre = db.Genres.ToList().Find(g => g.Id == genre.Id);
                if(string.IsNullOrWhiteSpace(genre.Name))
                {
                    TempData["Error"] = "Missing Name";
                    return RedirectToAction("GenreManage");
                }
                genre.Name = genre.Name.Trim();
                if (oldgenre.Name.ToLower() == genre.Name.ToLower() || db.Genres.ToList().Find(g => g.Name.ToLower() == genre.Name.ToLower()) == null)
                {
                    oldgenre.Name = genre.Name;
                    UpdateGenreMovies(oldgenre, SelectedMovies);
                    db.Entry(oldgenre).State = EntityState.Modified;

                    db.SaveChanges();
                    TempData["Success"] = genre.Name + " updated";
                }
                else
                    TempData["Error"] = genre.Name + " already exists";
                            
            }
            if (Request.Form["delete"] != null)
            {
                genre = db.Genres.ToList().Find(g => g.Id == genre.Id);
                if (genre.Movies.Count() == 0)
                {
                    db.Genres.Attach(genre);
                    db.Genres.Remove(genre);
                    db.SaveChanges();
                    TempData["Success"] = genre.Name + " deleted";
                }                    
                else
                    TempData["Error"] = genre.Name + " must not have any movies in order to be deleted";
            }
            return RedirectToAction("GenreManage");
        }

        

        //*****************MOVIES********************
        public ActionResult GetMovie(int movieId)
        {
            Movie movie = db.Movies.ToList().Find(m => m.Id == movieId);
            if (movie == null)
                return RedirectToAction("Index"); //ERROR MOVIE NOT FOUND
            return PartialView(movie);
        }

        public JsonResult Search(string search)
        {
            List<Movie> movies_list = db.Movies.ToList().FindAll(m => m.Title.ToLower().Contains(search.ToLower()));
            List<MovieViewModel> movies = new List<MovieViewModel>();
            foreach (var m in movies_list)
                movies.Add(new MovieViewModel(m.Id, m.Title, m.Year));
            JsonResult result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            result.Data = movies;
            return result;
        }

        public JsonResult AllMovies()
        {
            List<Movie> movies_list = db.Movies.ToList();
            List<MovieViewModel> movies = new List<MovieViewModel>();
            foreach (var m in movies_list)
                movies.Add(new MovieViewModel(m.Id, m.Title, m.Year));
            JsonResult result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            result.Data = movies;
            return result;
        }

        [Authorize]
        public ActionResult MovieManage()
        {
            return View(db.Movies.ToList());
        }

        [Authorize]
        [HttpGet]
        public ActionResult MovieCreate()
        {
            var viewModel = new List<AssignedGenre>();
            foreach (var genre in db.Genres)
            {
                viewModel.Add(new AssignedGenre
                {
                    id = genre.Id,
                    Name = genre.Name,
                    Assigned = false
                });
            }
            ViewBag.Genres = viewModel;
            return PartialView();
        }

        [HttpPost]
        public ActionResult MovieCreate(Movie movie, string[] SelectedGenres)
        {
            if (string.IsNullOrWhiteSpace(movie.Title))
                TempData["Error"] = "Missing Title";
            else if (movie.Year <= 0)
                TempData["Error"] = "Invalid Year";
            else
            {
                movie.Title = movie.Title.Trim();
                if (db.Movies.ToList().Find(m => m.Title.ToLower() == movie.Title.ToLower() && m.Year == movie.Year) == null)
                {
                    //movie.Id = db.Movies.ToList().Max(m=>m.Id) + 1;                   
                    movie.Genres = new List<Genre>();
                    if (SelectedGenres != null)
                    {
                        foreach (var idstring in SelectedGenres)
                        {
                            int id = Convert.ToInt32(idstring);
                            movie.Genres.Add(db.Genres.ToList().Find(g => g.Id == id));
                        }
                    }

                    db.Movies.Add(movie);
                    db.SaveChanges();
                    TempData["Success"] = String.Format("{0} ({1}) created", movie.Title, movie.Year);
                }
                else
                {
                    TempData["Error"] = String.Format("{0} ({1}) already exists", movie.Title, movie.Year);
                }
            }
            return RedirectToAction("MovieManage");
        }

        [HttpGet]
        [Authorize]     
        public ActionResult MovieEdit(int movieId)
        {
            Movie movie = db.Movies.ToList().Find(m => m.Id == movieId);
            PopulateGenres(movie);
            return PartialView(movie);
        }

        public ActionResult MovieEdit(Movie movie, string[] SelectedGenres)
        {
            if (Request.Form["save"] != null)
            {
                Movie oldmovie = db.Movies.ToList().Find(m => movie.Id == m.Id);
                if (string.IsNullOrWhiteSpace(movie.Title))
                {
                    TempData["Error"] = "Missing Name";
                    return RedirectToAction("MovieManage");
                }
                if (movie.Year <= 0)
                {
                    TempData["Error"] = "Invalid Year";
                    return RedirectToAction("MovieManage");
                }
                movie.Title = movie.Title.Trim();
                //if ((oldmovie.Title == movie.Title && oldmovie.Year == movie.Year )|| db.Movies.ToList().Find(m => m.Title== movie.Title
                //    && m.Year == movie.Year) == null)
                Movie search = db.Movies.ToList().Find(m => m.Title.ToLower() == movie.Title.ToLower() && m.Year == movie.Year);
                if(search == null || search.Id == movie.Id)  //if new title,year isn't already taken or if title and year not changed
                {
                    oldmovie.Title = movie.Title;
                    oldmovie.Year = movie.Year;
                    UpdateMovieGenres(oldmovie, SelectedGenres);
                    db.Entry(oldmovie).State = EntityState.Modified;

                    db.SaveChanges();
                    TempData["Success"] = String.Format("{0} ({1}) updated", movie.Title, movie.Year);
                }
                else
                    TempData["Error"] = String.Format("{0} ({1}) already exists", movie.Title, movie.Year);

            }
            if (Request.Form["delete"] != null)
            {
                movie = db.Movies.ToList().Find(m => movie.Id == m.Id);

                db.Movies.Attach(movie);
                db.Movies.Remove(movie);
                db.SaveChanges();
                TempData["Success"] = String.Format("{0} ({1}) deleted", movie.Title, movie.Year);
            }
            return RedirectToAction("MovieManage");
        }

        private void PopulateGenres(Movie movie)
        {
            var genres= db.Genres;
            var moviegenres = new HashSet<int>(movie.Genres.Select(g => g.Id));
            var viewModel = new List<AssignedGenre>();
            foreach (var genre in genres)
            {
                viewModel.Add(new AssignedGenre
                {
                    id = genre.Id,
                    Name = genre.Name,
                    Assigned = moviegenres.Contains(genre.Id)
                });
            }
            ViewBag.Genres = viewModel;
        }

        private void UpdateMovieGenres(Movie movie, string[] SelectedGenres)
        {
            if (SelectedGenres == null)
            {
                movie.Genres.Clear();
                return;
            }

            var selected = new HashSet<string>(SelectedGenres);
            var moviegenres = new HashSet<int>(movie.Genres.Select(g => g.Id));
            foreach (var genre in db.Genres)
            {
                if (selected.Contains(genre.Id.ToString()))
                {
                    if (!moviegenres.Contains(genre.Id))
                    {
                        movie.Genres.Add(genre);
                    }
                }
                else
                {
                    if (moviegenres.Contains(genre.Id))
                    {
                        movie.Genres.Remove(genre);
                    }
                }
            }
        }

        //*****************USERS********************
        ApplicationDbContext context = new ApplicationDbContext();
        PasswordHasher passwordHash = new PasswordHasher();

        [Authorize]
        public ActionResult UserManage()
        {  
            return View(context.Users.ToList());
        }

        [Authorize]
        [HttpGet]
        public ActionResult UserCreate()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult UserCreate(ApplicationUser user, string password)
        {
            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                TempData["Error"] = "Missing Name";
            }
            else if(password.Length == 0)
            {
                TempData["Error"] = "Missing Password";
            }
            else
            {
                user.UserName = user.UserName.Trim();
                if (context.Users.ToList().Find(u => u.UserName == user.UserName) == null)
                {
                    user.PasswordHash = passwordHash.HashPassword(password);
                    context.Users.Add(user);                    
                    context.SaveChanges();

                    TempData["Success"] = user.UserName + " created";
                }
                else
                {
                    TempData["Error"] = user.UserName + " already exists";
                }

            }
            return RedirectToAction("UserManage");
        }

        [Authorize]
        [HttpGet]
        public ActionResult UserEdit(string userId)
        {            
            var user = context.Users.ToList().Find(u => u.Id == userId.ToString());
            return PartialView(user);
        }

        public ActionResult UserEdit(ApplicationUser user, string password)
        {
            if (Request.Form["save"] != null)
            {
                ApplicationUser olduser = context.Users.ToList().Find(u => u.Id == user.Id);
                if (string.IsNullOrWhiteSpace(user.UserName))
                {
                    TempData["Error"] = "Missing Username";
                    return RedirectToAction("UserManage");
                }
                user.UserName = user.UserName.Trim();
                if (olduser.UserName == user.UserName || context.Users.ToList().Find(u => u.UserName == user.UserName) == null)
                {
                    olduser.UserName = user.UserName;
                    if(password.Length != 0)
                        olduser.PasswordHash = passwordHash.HashPassword(password);
                    context.Entry(olduser).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["Success"] = user.UserName + " updated";
                }
                else
                    TempData["Error"] = user.UserName + " already exists";

            }
            if (Request.Form["delete"] != null)
            {
                if (user.UserName != User.Identity.Name)
                {
                    context.Users.Attach(user);
                    context.Users.Remove(user);
                    context.SaveChanges();
                    TempData["Success"] = user.UserName + " deleted";
                }
                else
                    TempData["Error"] =  "Cannot delete current account";
            }
            return RedirectToAction("UserManage");
        }

    }
    
}

