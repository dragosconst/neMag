using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace neMag.Models
{
    public class OrderContent
    {
        public int OrderContentId { get; set; }

        public int Quantity { get; set; }

        public double Total { get; set; }

        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }
}