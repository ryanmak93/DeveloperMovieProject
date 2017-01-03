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
    public class GenreController : Controller
    {
        MovieContext db = new MovieContext();

        public ActionResult GenreCheck(string name, string id)
        {
            if (id == "")
                return Json(!db.Genres.Any(g => g.Name == name), JsonRequestBehavior.AllowGet);
            return Json(!db.Genres.Any(g => g.Name == name && g.Id.ToString() != id), JsonRequestBehavior.AllowGet);
        }

        public ActionResult All(int id)
        {
            Genre genre = db.Genres.First(g => g.Id == id);
            return View(genre);
        }

        [Authorize]
        public ActionResult Manage()
        {
            return View(db.Genres.ToList());
        }

        [Authorize]
        [HttpGet]
        public ActionResult Create()
        {
            //create empty movielist for a list of unchecked movies
            ViewBag.Movies = db.Movies.Select(
                m => new AssignedMovie
                {
                    id = m.Id,
                    Title = m.Title,
                    Year = m.Year,
                    Assigned = false
                }).ToList();

            return View();
        }

        [HttpPost]
        public ActionResult Create(Genre genre, string[] SelectedMovies)
        {
            if (ModelState.IsValid)
            {
                genre.Movies = new List<Movie>();
                if(SelectedMovies != null)
                    genre.Movies = db.Movies.Where(m => SelectedMovies.Contains(m.Id.ToString())).ToList();
                db.Genres.Add(genre);
                db.SaveChanges();
                TempData["Success"] = genre.Name + " created";
            }
            return RedirectToAction("Manage");
        }

        [Authorize]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            Genre genre = db.Genres.First(g => g.Id == id);
            PopulateMovies(genre);
            return View(genre);
        }

        public ActionResult Edit(Genre genre, string[] SelectedMovies)
        {
            if (ModelState.IsValid)
            {
                Genre oldgenre = db.Genres.First(g => g.Id == genre.Id); //get old genre information

                //update genre info
                oldgenre.Name = genre.Name;
                UpdateGenreMovies(oldgenre, SelectedMovies); //update genre's movies
                db.Entry(oldgenre).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = genre.Name + " updated";
            }
            RouteData.Values.Remove("id"); //remove route value from url
            return RedirectToAction("Manage"); 
        }

        public ActionResult Delete(int id)
        {
            Genre genre = db.Genres.First(g => g.Id == id);
            db.Genres.Attach(genre);
            db.Genres.Remove(genre);
            db.SaveChanges();
            TempData["Success"] = genre.Name + " deleted";
            RouteData.Values.Remove("id");  //remove route value from url
            return Json(Url.Action("Manage", "Genre"), JsonRequestBehavior.AllowGet); //return to manage page via javascript
        }

        //Take a genre and use its list of movies to generate a 
        // list of selected movies
        private void PopulateMovies(Genre genre)
        {
            //go through all movies in database and record whether or not each one is part of the genre
            ViewBag.Movies = db.Movies.Select(
                m => new AssignedMovie {
                    id = m.Id,
                    Title = m.Title,
                    Year = m.Year,
                    Assigned = db.Genres.FirstOrDefault(g => g.Id == genre.Id).Movies.Any(movie => movie.Id == m.Id)
                }).ToList();
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

            var selected = new HashSet<int>(SelectedMovies.Select(int.Parse).ToList());
            genre.Movies = genre.Movies.Union(db.Movies.Where(m => selected.Contains(m.Id))).Except(db.Movies.Where(m => !selected.Contains(m.Id))).ToList();
        }


    }
}