using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.Entities
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string ProductName { get; set; }
        [Range(1, 1000)]
        public int UnitPrice { get; set; }
        public int QuantityAtHand { get; set; }
    }
}
