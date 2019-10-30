using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.Entities
{
    public class OrderLineItem
    {
        public int Id { get; set; }
        public Order Order { get; set; }
        public int OrderId { get; set; }
        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }
        public int Total { get; set; }
        public Product Product{ get; set; }
    }
}
