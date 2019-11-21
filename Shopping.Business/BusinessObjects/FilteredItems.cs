using System;
using System.Collections.Generic;
using System.Text;

namespace Shopping.Business.Entities
{
    public class FilteredItems
    {
        public List<OrderLineItemBO> InvalidItems { get; set; }
        public List<OrderLineItemBO> ValidItems { get; set; }
    }
}
