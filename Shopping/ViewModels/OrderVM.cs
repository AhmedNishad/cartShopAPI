using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.ViewModels
{
    public class OrderVM
    {
        public int Id { get; set; }
        public int Total { get; set; }
        public CustomerVM Customer { get ; set; }
        [Required]
        public DateTime Date { get; set; }
        [MinLength(1)]
        public List<OrderLineItemVM> LineItems { get; set; }
    }
}
