using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace neMag.Models
{
    public class Photo
    {
        [Key]
        public int PhotoId { get; set; }

        public string Path { get; set; } // relative to the project directory

        public string Name { get; set; }

        public string Extension { get; set; }

        public int? ProductId { get; set; }

        public int? PostId { get; set; }

    }
}