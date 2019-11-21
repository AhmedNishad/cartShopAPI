using Shopping.Business.Entities;
using Shopping.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.ViewModels
{
    public class OrdersViewModel
    {
        public List<OrderVM> Orders{ get; set; }
        public List<CustomerVM> Customers { get; set; }
        public string SelectedCustomerName { get; set; }
        public bool Next { get; set;}
        public int PageNumber { get; set; }
        public int SortBy { get; set; }
        public bool SuccessfullyAdded { get; set; }
        public bool SuccessfullyUpdated { get; set; }
        public bool SuccessfullyDeleted { get; set; }
    }
}
