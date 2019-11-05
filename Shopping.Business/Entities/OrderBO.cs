using AutoMapper;
using AutoMapper.Configuration.Annotations;
using Shopping.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.Business.Entities
{
    public class OrderBO
    {
        public int Id { get; set; }
        public int Total { get; set; }
        public CustomerBO Customer { get ; set; }
        [Required]
        public DateTime Date { get; set; }
        [MinLength(1)]
        public List<OrderLineItemBO> LineItems { get; set; }
    }
}
