using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.Data.Entities
{
    public class CustomerDO
    {
        [Key]
        public int CustomerId { get; set; }
        [Required]
        [MaxLength(50)]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage ="Please enter valid name")]
        public string Name { get; set; }
    }
}
