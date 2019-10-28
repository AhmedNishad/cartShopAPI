using Shopping.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.Models
{
    public class IndexViewModel
    {
        public List<Customer> Customers { get; set; }
        public DateTime Date { get; set; }
        public Customer AssociatedCustomer { get; set; }
        public List<Product> Products { get; set; }
        public List<OrderLineItem> LineItems { get; set; }
        public Order AssociatedOrder { get; set; }
    }
}
