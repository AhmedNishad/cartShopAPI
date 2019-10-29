using Shopping.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.Models
{
    public class OrdersViewModel
    {
        public List<Order> Orders{ get; set; }
        public List<Customer> Customers { get; set; }
    }
}
