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
    [AutoMap(typeof(CustomerDO))]
    public class CustomerBO
    {
        [SourceMember("CustomerId")]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage ="Please enter valid name")]
        public string Name { get; set; }
    }
}
