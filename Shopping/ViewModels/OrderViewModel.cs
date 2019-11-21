using Shopping.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.ViewModels
{
    public class OrderViewModel
    {
        public OrderVM Order { get; set; }
        public List<OrderLineItemVM> LineItems { get; set; }
        public List<ProductVM> Products { get; set; }
        public bool SuccessfullyUpdated { get; set; }
    }
}
