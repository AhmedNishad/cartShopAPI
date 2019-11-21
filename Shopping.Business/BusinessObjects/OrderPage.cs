using System;
using System.Collections.Generic;
using System.Text;

namespace Shopping.Business.Entities
{
    public class OrderPage
    {
        public int OrdersCount { get; set; }
        public List<OrderBO> Orders { get; set; }
        public int Skip { get; set; }
    }
}
