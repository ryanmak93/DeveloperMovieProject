using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;

namespace MovieApp.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public virtual ICollection<Genre> Genres { get; set; }

        public Movie() { }

        public Movie(string title, int year)
        {
            Title = title;
            Year = year;
            Genres = new List<Genre>();
        }

    }   

}