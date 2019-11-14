using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int Total { get; set; }
        public Customer Customer { get ; set; }
        [Required]
        public DateTime Date { get; set; }
        [MinLength(1)]
        public List<OrderLineItem> LineItems { get; set; }
    }
}
