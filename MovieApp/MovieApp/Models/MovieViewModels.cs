using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MovieApp.Models
{
    //For autocomplete (JSON does not like circular references caused by many-to-many relationships)
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

    //For assigning genres to movies on movie creation and editing
    public class AssignedGenre
    {
        public int id { get; set; }
        public string Name { get; set; }
        public bool Assigned { get; set; }
    }

    //For assigning movies to genres on movie creation and editing
    public class AssignedMovie
    {
        public int id { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public bool Assigned { get; set; }
    }

}