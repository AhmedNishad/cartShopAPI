using Microsoft.AspNetCore.Mvc.ModelBinding;
using Shopping.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.ViewModels
{
    public class IndexViewModel
    {
        public List<CustomerVM> Customers { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public CustomerVM AssociatedCustomer { get; set; }
        public List<ProductVM> Products { get; set; }
        public List<OrderLineItemVM> LineItems { get; set; }
        public OrderVM AssociatedOrder { get; set; }
        public bool QuantityError { get; set; }
    }
}
