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
        
        public int ParentId { get; set; }

        [Required(ErrorMessage = "Title is mandatory!")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is mandatory!")]
        public string Description { get; set; }


        public virtual Category Parent { get; set; }
    }
}
