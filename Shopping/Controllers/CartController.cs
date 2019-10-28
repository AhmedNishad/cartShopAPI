using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopping.Entities;
using Shopping.Models;
using Shopping.Services;

namespace Shopping.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartData cartData;

        public CartController(ICartData cartData)
        {
            this.cartData = cartData;
        }
        [HttpGet]
        public IActionResult Index()
        {
            var model = new IndexViewModel();
            model.Customers = cartData.GetCustomers();
            model.Products = cartData.GetProducts();
            return View(model);
        }
        [HttpPost]
        public IActionResult Index(int customerId, DateTime date, List<OrderLineItem> lineItems)
        {
            int result = cartData.AddOrder(customerId, date, lineItems);
            if (result == 0)
                return NotFound();
            return RedirectToAction("Index", "Home");
        }
    }
}