using System;
using System.Collections.Generic;
using System.Text;

namespace Shopping.Business.Entities
{
    public class OrderAddedResult
    {
        public List<OrderLineItemBO> InvalidItems{ get; set; }
        public bool Updated { get; set; }
    }
}
