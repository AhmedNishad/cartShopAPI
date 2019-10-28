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
        [Required]
        public Order Order { get; set; }
        public int Quantity { get; set; }
        public int Total { get; set; }
        [Required]
        public Product Product{ get; set; }
    }
}
