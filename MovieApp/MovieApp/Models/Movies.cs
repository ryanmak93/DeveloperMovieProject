using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;
using System.Web.Mvc;

namespace MovieApp.Models
{
    public class Movie
    {
        //[Remote("MovieCheck", "Movie", AdditionalFields = "Title, Year", ErrorMessage = "Movie already exists")]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
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