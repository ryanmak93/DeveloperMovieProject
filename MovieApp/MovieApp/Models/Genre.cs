using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Web.Mvc;

namespace MovieApp.Models
{
    public class Genre
    {
        public int Id { get; set; }

        [Required]
        [Remote("GenreCheck", "Genre", AdditionalFields = "Id", ErrorMessage = "Genre already exists")]
        public string Name { get; set; }

        public virtual ICollection<Movie> Movies { get; set; } 

        public Genre() { }

        public Genre(string name)
        {
            Name = name;
            Movies = new List<Movie>();
        }
    }


}