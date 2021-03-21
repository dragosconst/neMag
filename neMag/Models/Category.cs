using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace neMag.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Title is mandatory!")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Trebuie introdusa o denumire pt categorie")]
        public string CategoryName { get; set; }

        [Required(ErrorMessage = "Description is mandatory!")]
        public string Description { get; set; }

        // public virtual ICollection<Topic> Topics { get; set; }
    }
}
