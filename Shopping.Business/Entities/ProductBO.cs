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
    [AutoMap(typeof(ProductDO))]
    public class ProductBO
    {
        [SourceMember("ProductId")]
        public int Id { get; set; }
        [Required]
        public string ProductName { get; set; }
        public int UnitPrice { get; set; }
        [Range(1, 10000)]
        public int QuantityAtHand { get; set; }
    }
}
