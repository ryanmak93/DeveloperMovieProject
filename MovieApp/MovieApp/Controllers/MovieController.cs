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

        [Authorize]
        public ActionResult Manage()
        {
            return View();
        }

        //*****************GENRES********************

        //All movies in a genre
        public ActionResult GenreAll(string name)
        {
            Genre genre = db.Genres.ToList().Find(g => g.Name == name);
            return View(genre);
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
            //create empty movielist for a list of unchecked movies
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
            genre.Name = genre.Name.Trim();
            if (db.Genres.ToList().Find(g => g.Name.ToLower() == genre.Name.ToLower()) == null) // See genre already exists
            {
                // add selected movies to new genre object
                genre.Movies = new List<Movie>();
                if (SelectedMovies != null)
                {
                    foreach (var idstring in SelectedMovies)
                    {
                        int id = Convert.ToInt32(idstring);
                        genre.Movies.Add(db.Movies.ToList().Find(m => m.Id == id));
                    }
                }

                //add to database
                db.Genres.Add(genre);
                db.SaveChanges();
                TempData["Success"] = genre.Name + " created";
            }
            else
            {
                TempData["Error"] = genre.Name + " already exists";
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
       
        //Take a genre and use its list of movies to generate a 
        // list of selected movies
        private void PopulateMovies(Genre genre)
        {
            var movies = db.Movies;
            var genremovies = new HashSet<int>(genre.Movies.Select(m => m.Id)); //hash of movieIds in a genre for fast lookup
            var viewModel = new List<AssignedMovie>();

            //go through all movies in database and record whether or not each one is part of the genre
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

        //takes all the selected movies as id strings and updates the genre's movies
        private void UpdateGenreMovies(Genre genre, string[] SelectedMovies)
        {
            //make a new, empty list if no genres were chosen
            if (SelectedMovies == null)
            {
                genre.Movies.Clear();
                return;
            }       

            var selected = new HashSet<string>(SelectedMovies);
            var genremovies = new HashSet<int>(genre.Movies.Select(m => m.Id));
            foreach (var movie in db.Movies)
            {
                //if a movie is selected, add it; remove otherwise
                if (selected.Contains(movie.Id.ToString()))
                {
                    if (!genremovies.Contains(movie.Id))
                        genre.Movies.Add(movie);
                }
                else
                {
                    if (genremovies.Contains(movie.Id))
                        genre.Movies.Remove(movie);
                }
            }
        }

        public ActionResult GenreEdit(Genre genre, string[] SelectedMovies)
        {           
            if (Request.Form["save"] != null)
            {                
                Genre oldgenre = db.Genres.ToList().Find(g => g.Id == genre.Id); //get old genre information
                genre.Name = genre.Name.Trim();
                if (oldgenre.Name.ToLower() == genre.Name.ToLower() || db.Genres.ToList().Find(g => g.Name.ToLower() == genre.Name.ToLower()) == null) //check if new name in use
                {

                    //update genre info
                    oldgenre.Name = genre.Name;
                    UpdateGenreMovies(oldgenre, SelectedMovies); //update genre's movies
                    db.Entry(oldgenre).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = genre.Name + " updated";
                }
                else
                    TempData["Error"] = genre.Name + " already exists";
                            
            }
            if (Request.Form["delete"] != null) //delete genre
            {
                genre = db.Genres.ToList().Find(g => g.Id == genre.Id);
                if (genre.Movies.Count() == 0) //only delete if it contains no movies
                {
                    //update database
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
        public ActionResult GetMovie(int movieId) // get information for movie given id
        {
            Movie movie = db.Movies.ToList().Find(m => m.Id == movieId);
            return PartialView(movie);
        }

        public JsonResult Search(string search) //for autocomplete
        {
            List<Movie> movieList = db.Movies.ToList().FindAll(m => m.Title.ToLower().Contains(search.ToLower())); //get all movies with title containing search term
            List<MovieViewModel> movies = new List<MovieViewModel>();
            foreach (var m in movieList)
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
            movie.Title = movie.Title.Trim();
            if (db.Movies.ToList().Find(m => m.Title.ToLower() == movie.Title.ToLower() && m.Year == movie.Year) == null) //check if movie already exists
            {              
                movie.Genres = new List<Genre>();

                //populate genre list
                if (SelectedGenres != null)
                {
                    foreach (var idstring in SelectedGenres)
                    {
                        int id = Convert.ToInt32(idstring);
                        movie.Genres.Add(db.Genres.ToList().Find(g => g.Id == id));
                    }
                }

                //add to database
                db.Movies.Add(movie);
                db.SaveChanges();
                TempData["Success"] = String.Format("{0} ({1}) created", movie.Title, movie.Year);
            }
            else
            {
                TempData["Error"] = String.Format("{0} ({1}) already exists", movie.Title, movie.Year);
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
                Movie oldmovie = db.Movies.ToList().Find(m => movie.Id == m.Id); //get old movie information
                movie.Title = movie.Title.Trim();
                Movie search = db.Movies.ToList().Find(m => m.Title.ToLower() == movie.Title.ToLower() && m.Year == movie.Year);
                if(search == null || search.Id == movie.Id)  //if new title,year isn't already taken or if title and year not changed
                {
                    //update movie in database 
                    oldmovie.Title = movie.Title;
                    oldmovie.Year = movie.Year;
                    UpdateMovieGenres(oldmovie, SelectedGenres); //add selected genres to movie's genres
                    db.Entry(oldmovie).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = String.Format("{0} ({1}) updated", movie.Title, movie.Year);
                }
                else
                    TempData["Error"] = String.Format("{0} ({1}) already exists", movie.Title, movie.Year);

            }
            if (Request.Form["delete"] != null)//delete movie from database
            {
                movie = db.Movies.ToList().Find(m => movie.Id == m.Id);
                db.Movies.Attach(movie);
                db.Movies.Remove(movie);
                db.SaveChanges();
                TempData["Success"] = String.Format("{0} ({1}) deleted", movie.Title, movie.Year);
            }
            return RedirectToAction("MovieManage");
        }

        //Take a movie and use its list of genres to generate a 
        // list of selected genres
        private void PopulateGenres(Movie movie)
        {
            var genres= db.Genres;
            var moviegenres = new HashSet<int>(movie.Genres.Select(g => g.Id)); //hash of genreIds in a genre for fast lookup
            var viewModel = new List<AssignedGenre>();

            //go through all genres in database and record whether or not each one contains the movie
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

        //takes all the selected genres as id strings and updats the movie's genres
        private void UpdateMovieGenres(Movie movie, string[] SelectedGenres)
        {
            //make a new, empty list if no movies were chosen
            if (SelectedGenres == null)
            {
                movie.Genres.Clear();
                return;
            }

            var selected = new HashSet<string>(SelectedGenres);
            var moviegenres = new HashSet<int>(movie.Genres.Select(g => g.Id));
            foreach (var genre in db.Genres)
            {
                //if a genre is selected, add it; remove otherwise
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
        ApplicationDbContext context = new ApplicationDbContext(); //context which contains the application users
        PasswordHasher passwordHash = new PasswordHasher(); //for hashing password

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
            user.UserName = user.UserName.Trim();
            if (context.Users.ToList().Find(u => u.UserName.ToLower() == user.UserName.ToLower()) == null) //check if username is taken
            {
                //add user to database
                user.PasswordHash = passwordHash.HashPassword(password); //hashes the given password
                user.SecurityStamp = Guid.NewGuid().ToString(); //required by ASP.NET Identity 
                context.Users.Add(user);
                context.SaveChanges();
                TempData["Success"] = user.UserName + " created";
            }
            else
            {
                TempData["Error"] = user.UserName + " already exists";
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

        [HttpPost]
        public ActionResult UserEdit(ApplicationUser user, string password)
        {
            if (Request.Form["save"] != null)
            {
                ApplicationUser olduser = context.Users.ToList().Find(u => u.Id == user.Id);
                user.UserName = user.UserName.Trim();
                if (olduser.UserName == user.UserName || context.Users.ToList().Find(u => u.UserName.ToLower() == user.UserName.ToLower()) == null) // check if new username is taken
                {
                    //update user in database
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
            if (Request.Form["delete"] != null) //remove user from database
            {
                if (user.UserName != User.Identity.Name) //cannot delete current account
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

        [Authorize]
        public ActionResult ResetDatabase()
        {
            ClearData();
            AddTestData();       
            return RedirectToAction("Index"); 
        }

        private void ClearData()//delete all rows in movies and genres
        {
            foreach (var genre in db.Genres)
            {
                db.Genres.Attach(genre);
                db.Genres.Remove(genre);

            }
            foreach (var movie in db.Movies)
            {
                db.Movies.Attach(movie);
                db.Movies.Remove(movie);

            }
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

