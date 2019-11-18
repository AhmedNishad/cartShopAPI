using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.API.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage ="Please enter valid name")]
        public string Name { get; set; }
    }
}
