using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Shopping.Entities;
using Shopping.Models;
using Shopping.Business.Services;
using AutoMapper;
using Shopping.Business.Entities;
using Shopping.Business;

namespace Shopping.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IProductService productService;
        private readonly IOrderService orderService;
        private readonly IMapper mapper;

        private ICustomerService customerService { get; }

        public OrderController(IProductService productService, ICustomerService customerService, 
            IOrderService orderService, IMapper mapper)
        {
            this.productService = productService;
            this.customerService = customerService;
            this.orderService = orderService;
            this.mapper = mapper;
        }
        [HttpGet]
        public IActionResult Index(List<OrderLineItem> lineItems = null)
        {
            var model = new IndexViewModel();
            model.LineItems = lineItems;
            model.Customers = mapper.Map<List<Customer>>(customerService.GetCustomers());
            model.Products = mapper.Map<List<Product>>(productService.GetProducts());
            return View(model);
        }
        [HttpPost]
        public IActionResult AddOrder(DateTime date, int customerId, List<OrderLineItem> lineItems)
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
                    item.Product = mapper.Map<Product>(productService.GetProductById(item.Product.Id));
                    item.LinePrice = item.Product.UnitPrice;
                }

                date.AddHours(DateTime.Now.Hour);

                var results = mapper.Map<List<OrderLineItem>>(orderService.AddOrder(customerId, date, 
                    mapper.Map<List<OrderLineItemBO>>(lineItems)));
                var last = new OrderLineItem();
                // If there is only one order remaining in the returned list of orders there are inadequate
                //      products for just this product ( Errorr thrown at BL )
                if (results.Count != 0)
                {
                    // Container to get the created orderId
                    last = results[results.Count - 1];
                }
                // Remove placeholder orderID element
                results.Remove(last);
                // If there are many line items with quantity too high redirect to add order with order items entered 
                //with appropriate quantities
                if (results.Count > 0)
                {
                    var model = new IndexViewModel();
                    model.LineItems = results;
                    model.Customers = mapper.Map<List<Customer>>(customerService.GetCustomers());
                    model.Date = date;
                    model.Products = mapper.Map<List<Product>>(productService.GetProducts());
                    return View("Index",model);
                }

                return RedirectToAction("ViewOrdersForCustomer", new {  customerId, addedSuccessfully=true });
            } catch (Exception e)
            {
                return View("ErrorDisplay", new ErrorModel { Message = e.Message });
            }
        }
        [HttpGet]
        public IActionResult ViewOrder(int orderId, bool updated = false)
        {
            var model = new OrderViewModel();
            if (updated)
            {
                model.SuccessfullyUpdated = true;
            }
            model.LineItems = mapper.Map<List<OrderLineItem>>(orderService.GetLineItemsForOrder(orderId));
            model.Order = mapper.Map<Order>(orderService.GetOrderById(orderId));
            model.Products = mapper.Map<List<Product>>(productService.GetProducts());
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

        //[HttpPost]
        //public IActionResult UpdateLineItem(int lineId, OrderLineItem orderLineItem, int productId)
        //{
        //    orderLineItem.Product = mapper.Map<Product>(productService.GetProductById(productId));
        //    orderLineItem.Total = orderLineItem.Product.UnitPrice * orderLineItem.Quantity;
        //    int result = orderService.UpdateLineItem(lineId, mapper.Map<OrderLineItemBO>(orderLineItem));
        //    if (result < 0)
        //    {
        //        return View("ErrorDisplay", new ErrorModel { Message = $"There are inadequate products, you have requested {-1 * result} more than we capacity for {orderLineItem.Product.ProductName}" });
        //    }
        //    return RedirectToAction("ViewOrder", new { orderId = result });
        //}

        [HttpPost]
        public IActionResult UpdateOrder(List<OrderLineItem> lineItems, int orderID)
        {
            try
            {
                orderService.UpdateLineItemsForOrder(mapper.Map<List<OrderLineItemBO>>(lineItems), orderID);
            }catch(Exception e)
            {
                return View("ErrorDisplay", new ErrorModel { Message = e.Message });
            }
            return RedirectToAction("ViewOrder",new { orderId = orderID, updated=true });
        }

        public IActionResult ViewOrders(int pageNumber, string sortBy, bool successfullyDeleted = false)
        {
            var model = new OrdersViewModel();
            if (successfullyDeleted)
            {
                model.SuccessfullyDeleted = true;
            }
            model.Customers = mapper.Map<List<Customer>>(customerService.GetCustomers());
            var OrderPage = orderService.GetOrders(pageNumber, sortBy);

            model.Orders = mapper.Map<List<Order>>(OrderPage.Orders);
            model.Next = AreThereRemainingOrders(pageNumber, OrderPage.OrdersCount, OrderPage.Skip);
            model.SortBy = sortBy;
            model.PageNumber = pageNumber;
            return View(model);
        }

        private bool AreThereRemainingOrders(int pageNumber,int totalOrders,int skipCount)
        {
            // Check if there enough orders to skip through
            if(((pageNumber+1) * skipCount) < totalOrders)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public IActionResult ViewOrdersForCustomer(int customerId, bool addedSuccessfully = false)
        {
            // If customer isn't found. Send back to all orders
            if(customerId == 0)
            {
                return RedirectToAction("ViewOrders", new { pageNumber = 0 });
            }
            var model = new OrdersViewModel();
            if (addedSuccessfully)
            {
                model.SuccessfullyAdded = true;
            }
            model.SelectedCustomerName = mapper.Map<Customer>(customerService.GetCustomerById(customerId)).Name;
            model.Orders = mapper.Map<List<Order>>(orderService.GetOrdersForCustomer(customerId));
            model.Customers = mapper.Map<List<Customer>>(customerService.GetCustomers());
            return View("ViewOrders", model);
        }

        // Unimplimented
        //[HttpGet]
        //public IActionResult DeleteLineItemById(int lineId, int orderID)
        //{
        //    int result = orderService.DeleteLineItemById(lineId);
        //    if (result == 0)
        //    {
        //        return RedirectToAction("ShowOrders");
        //    }
        //    return RedirectToAction("ViewOrder", new { orderId = orderID });
        //}

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
            int result = orderService.DeleteOrder(order.Id);
            if (result == 0)
            {
                return RedirectToAction("Index");
            }
            return RedirectToAction("ViewOrders", new { successfullyDeleted = true});
            
        }
    }
}