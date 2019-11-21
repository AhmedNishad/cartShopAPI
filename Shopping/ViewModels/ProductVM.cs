using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.ViewModels
{
    public class ProductVM
    {
        public int Id { get; set; }
        [Required]
        public string ProductName { get; set; }
        public int UnitPrice { get; set; }
        [Range(1, 10000)]
        public int QuantityAtHand { get; set; }
    }
}
