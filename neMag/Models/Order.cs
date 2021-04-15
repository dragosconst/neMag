using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace neMag.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public string UserId { get; set; }

        public string Details { get; set; }

        public DateTime Date { get; set; }

        public double TotalPrice { get; set; }

        public string Status { get; set; }


        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<OrderContent> OrderContents { get; set; }
    }
}