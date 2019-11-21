using System;
using System.Collections.Generic;
using System.Text;

namespace Shopping.Business.Entities
{
    public class OrderUpdatedResult
    {
        public List<OrderLineItemBO> InvalidItems { get; set; }
        public DateTime Date { get; set; }
    }
}
