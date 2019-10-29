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
            date.AddHours(DateTime.Now.Hour);
            if (result == 0)
            {
                
                
                return NotFound();
            }
            return RedirectToAction("ViewOrder", new { orderId = result });
        }
        [HttpGet]
        public IActionResult ViewOrder(int orderId)
        {
            var model = new OrderViewModel();
            model.LineItems = cartData.GetLineItemsForOrder(orderId);
            model.Order = cartData.GetOrderById(orderId);
            model.Products = cartData.GetProducts();
            return View(model);
        }

        [HttpPost]
        public IActionResult ViewOrder(List<OrderLineItem> UpdatedItems)
        {
            int result = cartData.UpdateLineItems(UpdatedItems);
            if(result == 0)
            {
                return NotFound();
            }
            return RedirectToAction("ViewOrder", new { orderId = result });
        }

        [HttpPost]
        public IActionResult UpdateLineItem(int lineId, OrderLineItem orderLineItem, int productId)
        {
            orderLineItem.Product = cartData.GetProductById(productId);
            orderLineItem.Total = orderLineItem.Product.UnitPrice * orderLineItem.Quantity;
            int result = cartData.UpdateLineItem(lineId, orderLineItem);
            if(result == 0)
            {
                return NotFound();
            }
            return RedirectToAction("ViewOrder", new { orderId = result });
        }

        public IActionResult ViewOrders()
        {
            var model = new OrdersViewModel();
            model.Customers = cartData.GetCustomers();
            model.Orders = cartData.GetOrders();
            return View(model);
        }
    }
}