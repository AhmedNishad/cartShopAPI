using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shopping.API.Entities;
using Shopping.Business;
using Shopping.Business.Entities;

namespace Shopping.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private IMapper mapper;
        private ICustomerService customerService;
        public CustomersController(ICustomerService customerService, IMapper mapper)
        {
            this.mapper = mapper;
            this.customerService = customerService;
        }

        [HttpGet]
        public IActionResult GetCustomers()
        {
            return Ok(customerService.GetCustomers());
        }

        [HttpPost]
        public IActionResult PostCustomer(Customer customer)
        {
            try
            {
                int createdId = customerService.AddCustomer(mapper.Map<CustomerBO>(customer));
                return RedirectToAction("GetCustomer", new { id = createdId });
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetCustomer(int id) 
        {
            return Ok(customerService.GetCustomerById(id));
        }
    }
}