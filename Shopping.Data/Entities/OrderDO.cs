using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.Data.Entities
{
    public class OrderDO
    {
        [Key]

        public int OrderId { get; set; }
        public int Total { get; set; }
        public CustomerDO Customer { get ; set; }
        [Required]
        public DateTime Date { get; set; }
        [MinLength(1)]
        public List<OrderLineItemDO> LineItems { get; set; }
    }
}
