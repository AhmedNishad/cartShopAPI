using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.API.Entities
{
    public class OrderLineItem
    {
        public int Id { get; set; }
        public Order Order { get; set; }
        public int OrderId { get; set; }
        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }
        public int LinePrice { get; set; }
        
        public int Total { get { return Quantity * LinePrice; } }
        public Product Product{ get; set; }
    }
}
