using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace neMag.Models
{
    public class Product
    {
        [Key]
        [Required]
        public int ProductId { get; set; }

        public string UserId { get; set; }

        [Required(ErrorMessage = "Category is mandatory!")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Name is mandatory!")]
        public string ProductName { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Price is mandatory!")]
        public double Price { get; set; }

        public double Discount { get; set; }

        public double Rating { get; set; }

        public bool Accepted { get; set; }


        public virtual Category Category { get; set; }

        public virtual ApplicationUser User { get; set; }

        public IEnumerable<SelectListItem> Categ { get; set; }

        public virtual ICollection<Post> Posts { get; set; }

        public virtual IList<Photo> Photos { get; set; }

        //public virtual IEnumerable<UserProducts> UserProducts { get; set; }
    }
}