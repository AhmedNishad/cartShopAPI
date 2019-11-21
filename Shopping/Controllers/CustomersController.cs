using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopping.Business;
using Shopping.Business.Entities;
using Shopping.Business.Services;
using Shopping.ViewModels;
using Shopping.Models;

namespace Shopping.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private ICustomerService customerService { get; }
        private IMapper mapper;
        public CustomersController(ICustomerService customerService, IMapper mapper)
        {
            this.mapper = mapper;
            this.customerService = customerService;
        }
        [ValidateAntiForgeryToken]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult AddCustomer()
        {
            var model = new CustomerVM();
            return View(model);
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult AddCustomer(CustomerVM customer)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(customer);
                }
                int result = customerService.AddCustomer(mapper.Map<CustomerBO>(customer));
                
                return RedirectToAction("Index", "Order");
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