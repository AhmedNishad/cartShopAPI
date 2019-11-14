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
        public string SelectedCustomerName { get; set; }
        public bool Next { get; set;}
        public int PageNumber { get; set; }
        public string SortBy { get; set; }
        public bool SuccessfullyAdded { get; set; }
        public bool SuccessfullyDeleted { get; set; }
    }
}
