using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.Data.Entities
{
    public class OrderLineItemDO
    {
        [Key]
        public int LineId { get; set; }
        public int LinePrice { get; set; }
        public OrderDO Order { get; set; }
        public int OrderId { get; set; }
        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }
        public int Total 
        { get { return LinePrice * Quantity; } }
            
        public ProductDO Product{ get; set; }
    }
}
