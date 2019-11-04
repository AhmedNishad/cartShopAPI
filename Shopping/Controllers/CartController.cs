using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
        public IActionResult Index(List<OrderLineItem> lineItems = null)
        {
            var model = new IndexViewModel();
            model.LineItems = lineItems;
            model.Customers = cartData.GetCustomers();
            model.Products = cartData.GetProducts();
            return View(model);
        }
        [HttpPost]
        public IActionResult Index(DateTime date, int customerId, List<OrderLineItem> lineItems)
        {
            try
            {
                // Manual Validation. View Models To be implemented ... 
                if (customerId == 0)
                {
                    throw new Exception("Please Select a customer");
                }
                if (lineItems.Count < 1)
                {
                    throw new Exception("Please add line items");
                }
                if (date.ToShortDateString() == "1/1/0001")
                {
                    throw new Exception("Please Enter Date");
                }
                foreach (var item in lineItems)
                {
                    item.Product = cartData.GetProductById(item.Product.Id);
                    item.Total = item.Product.UnitPrice * item.Quantity;
                }

                date.AddHours(DateTime.Now.Hour);
                var results = cartData.AddOrder(customerId, date, lineItems);
                var last = new OrderLineItem();
                if (results.Count == 0)
                {
                    throw new Exception($"There are inadequate products");
                }
                else
                {
                    last = results[results.Count - 1];
                }
                results.Remove(last);
                if (results.Count > 0)
                {
                    var model = new IndexViewModel();
                    model.LineItems = results;
                    model.Customers = cartData.GetCustomers();
                    model.Date = date;
                    model.Products = cartData.GetProducts();
                    return View(model);
                }

                return RedirectToAction("ViewOrder", new { orderId = last.OrderId });
            } catch (Exception e)
            {
                return View("ErrorDisplay", new ErrorModel { Message = e.Message });
            }
        }
        [HttpGet]
        public IActionResult ViewOrder(int orderId)
        {
            var model = new OrderViewModel();
            // Aggregate all line Item Totals
            model.LineItems = cartData.GetLineItemsForOrder(orderId);
            model.Order = cartData.GetOrderById(orderId);
            model.Products = cartData.GetProducts();
            return View(model);
        }

        // Unimplimented
        //[HttpPost]
        //public IActionResult ViewOrder(List<OrderLineItem> UpdatedItems)
        //{
        //    int result = cartData.UpdateLineItems(UpdatedItems);
        //    if (result == 0)
        //    {
        //        return NotFound();
        //    }
        //    return RedirectToAction("ViewOrder", new { orderId = result });
        //}

        [HttpPost]
        public IActionResult UpdateLineItem(int lineId, OrderLineItem orderLineItem, int productId)
        {
            orderLineItem.Product = cartData.GetProductById(productId);
            orderLineItem.Total = orderLineItem.Product.UnitPrice * orderLineItem.Quantity;
            int result = cartData.UpdateLineItem(lineId, orderLineItem);
            if (result < 0)
            {
                return View("ErrorDisplay", new ErrorModel { Message = $"There are inadequate products, you have requested {-1 * result} more than we capacity for {orderLineItem.Product.ProductName}" });
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

        public IActionResult ViewOrdersForCustomer(int customerId)
        {
            var model = new OrdersViewModel();
            model.SelectedCustomerName = cartData.GetCustomerById(customerId).Name;
            model.Orders = cartData.GetOrdersForCustomer(customerId);
            model.Customers = cartData.GetCustomers();
            return View("ViewOrders", model);
        }

        public IActionResult ViewProducts()
        {
            var model = new ProductsViewModel();
            model.Products = cartData.GetProducts();
            return View(model);
        }

        [HttpGet]
        public IActionResult ViewProduct(int productId)
        {
            var model = new Product();
            model = cartData.GetProductById(productId);
            return View(model);
        }

        [HttpPost]
        public IActionResult ViewProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
                product.QuantityAtHand = 0;
                return View(product);
            }
            int result = cartData.QuantityUpdateForProduct(product.Id, product.QuantityAtHand);
            return RedirectToAction("ViewProduct", new { productId = result });
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            var model = new Product();

            return View(model);
        }

        [HttpPost]
        public IActionResult AddProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
                return View(product);
            }
            int result = cartData.AddProduct(product);
            return RedirectToAction("ViewProduct", new { productId = result });
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
                int result = cartData.AddCustomer(customer);
                if (result == 0)
                {
                    throw new Exception("Customer already exists");
                }
                return RedirectToAction("Index");
            } catch (Exception e)
            {
                var error = new ErrorModel();
                error.Message = e.Message;
                return View("ErrorDisplay", error);
            }
        }

        [HttpGet]
        public IActionResult DeleteLineItemById(int lineId, int orderID)
        {
            int result = cartData.deleteLineItemById(lineId);
            if (result == 0)
            {
                return RedirectToAction("ShowOrders");
            }
            return RedirectToAction("ViewOrder", new { orderId = orderID });
        }

        [HttpGet]
        public IActionResult DeleteOrder(int orderID)
        {
            return RedirectToAction("ConfirmDeleteOrder", new { orderId = orderID });
        }

        [HttpGet]
        public IActionResult ConfirmDeleteOrder(int orderId)
        {
            var model = new Order();
            model.Id = orderId;
            return View(model);
        }
        [HttpPost]
        public IActionResult ConfirmDeleteOrder(Order order)
        {
            int result = cartData.DeleteOrder(order.Id);
            if (result == 0)
            {
                return RedirectToAction("Index");
            }
            return RedirectToAction("ViewOrders");
            
        }
    }
}