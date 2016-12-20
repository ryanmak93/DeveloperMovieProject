using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MovieApp.Models
{
    public class MovieViewModel
    {
        public int MovieID;
        public string Title { get; set; }
        public int Year { get; set; }

        public MovieViewModel() { }

        public MovieViewModel(int id, string title, int year)
        {
            MovieID = id;
            Title = title;
            Year = year;
        }
    }

    public class MovieDatabase
    {
        public List<Movie> MovieList;
        public List<Genre> GenreList;
        public MovieDatabase() { }
        public MovieDatabase(List<Movie> movielist, List<Genre> genrelist)
        {
            MovieList = movielist;
            GenreList = genrelist;
        }
    }
    public class GenreViewModel
    {
        public string Name;
        public int GenreId;
        public GenreViewModel() { }

        public GenreViewModel(int id, string name)
        {
            Name = name;
            GenreId = id;
        }
    }
    public class AssignedGenre
    {
        public int id { get; set; }
        public string Name { get; set; }
        public bool Assigned { get; set; }
    }

    public class AssignedMovie
    {
        public int id { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public bool Assigned { get; set; }
    }

}