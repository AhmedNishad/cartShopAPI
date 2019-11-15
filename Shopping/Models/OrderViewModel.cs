using Shopping.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.Models
{
    public class OrderViewModel
    {
        public Order Order { get; set; }
        public List<OrderLineItem> LineItems { get; set; }
        public List<Product> Products { get; set; }
        public bool SuccessfullyUpdated { get; set; }
    }
}
