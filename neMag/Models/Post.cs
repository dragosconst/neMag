using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace neMag.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        [Required(ErrorMessage = "Content is mandatory!")]
        public string Content { get; set; }

        public bool isReview { get; set; }
        public int Rating { get; set; }

        public DateTime Date { get; set; }

       // public int ProductId { get; set; }

        //public virtual Product Product { get; set; }
    }
}