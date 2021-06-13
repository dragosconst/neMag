using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace neMag.Models
{
    public class UserProducts
    {
        [Key, Column(Order = 0)]
        public string UserId { get; set; }

        [Key, Column(Order = 1)]
        public int ProductId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual Product Product { get; set; }
    }
}