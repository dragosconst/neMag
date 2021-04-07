using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace online_shop.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public string Details { get; set; }
        public DateTime Date { get; set; }
        public double TotalPrice { get; set; }
        public string Status { get; set; }


        public virtual ApplicationUser User { get; set; }
    }
}