using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Shopping.Business.Entities;
using Shopping.Business.Services;
using Shopping.Entities;
using Shopping.Models;

namespace Shopping.Controllers
{
    public class CustomersController : Controller
    {
        private CustomerService customerService { get; }
        private IMapper mapper;
        public CustomersController(CustomerService customerService, IMapper mapper)
        {
            this.mapper = mapper;
            this.customerService = customerService;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult AddCustomer()
        {
            var model = new Customer();
            return View(model);
        }

        [HttpPost]
        public IActionResult AddCustomer(Customer customer)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(customer);
                }
                int result = customerService.AddCustomer(mapper.Map<CustomerBO>(customer));
                if (result == 0)
                {
                    throw new Exception("Customer already exists");
                }
                return RedirectToAction("Order","Index");
            }
            catch (Exception e)
            {
                var error = new ErrorModel();
                error.Message = e.Message;
                return View("ErrorDisplay", error);
            }
        }
    }
}