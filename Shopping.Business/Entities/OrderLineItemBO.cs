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
    [AutoMap(typeof(OrderLineItemDO))]
    public class OrderLineItemBO
    {
        [SourceMember("LineId")]
        public int Id { get; set; }
        public OrderBO Order { get; set; }
        public int OrderId { get; set; }
        public int LinePrice { get; set; }
        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }
        public int Total { get { return LinePrice * Quantity; } }
        public ProductBO Product{ get; set; }
    }
}
