using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Shopping.ViewModels;
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
        public IActionResult Index(List<OrderLineItemVM> lineItems = null)
        {
            var model = new IndexViewModel();
            model.LineItems = lineItems;
            model.Customers = mapper.Map<List<CustomerVM>>(customerService.GetCustomers());
            model.Products = mapper.Map<List<ProductVM>>(productService.GetProducts());
            return View(model);
        }
        [HttpPost]
        public IActionResult AddOrder(DateTime date, int customerId, List<OrderLineItemVM> lineItems)
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
                    item.Product = mapper.Map<ProductVM>(productService.GetProductById(item.Product.Id));
                    item.LinePrice = item.Product.UnitPrice;
                }

                date.AddHours(DateTime.Now.Hour);

                var addedResult = orderService.AddOrder(customerId, date,
                    mapper.Map<List<OrderLineItemBO>>(lineItems));
                var invalidItems = mapper.Map<List<OrderLineItemVM>>(addedResult.InvalidItems);
               
                // If there are many line items with quantity too high redirect to add order with order items entered 
                //with appropriate quantities
                if (invalidItems.Count > 0)
                {
                    var model = new IndexViewModel();
                    model.QuantityError = true;
                    model.LineItems = invalidItems;
                    model.Customers = mapper.Map<List<CustomerVM>>(customerService.GetCustomers());
                    model.Date = date;
                    model.Products = mapper.Map<List<ProductVM>>(productService.GetProducts());
                    return View("Index",model);
                }
                if (addedResult.Updated)
                {
                    return RedirectToAction("ViewOrdersForCustomer", new { customerId, updated = true });
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
            model.LineItems = mapper.Map<List<OrderLineItemVM>>(orderService.GetLineItemsForOrder(orderId));
            model.Order = mapper.Map<OrderVM>(orderService.GetOrderById(orderId));
            model.Products = mapper.Map<List<ProductVM>>(productService.GetProducts());
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
        public IActionResult UpdateOrder(List<OrderLineItemVM> lineItems, int orderID)
        {
            try
            {
                var updatedResult = orderService.UpdateLineItemsForOrder(mapper.Map<List<OrderLineItemBO>>(lineItems), orderID);
                var invalidItems = mapper.Map<List<OrderLineItemVM>>(updatedResult.InvalidItems);
                if (invalidItems.Count > 0)
                {
                    var model = new IndexViewModel();
                    model.QuantityError = true;
                    model.LineItems = invalidItems;
                    model.Customers = mapper.Map<List<CustomerVM>>(customerService.GetCustomers());
                    model.Date = updatedResult.Date;
                    model.Products = mapper.Map<List<ProductVM>>(productService.GetProducts());
                    return View("Index", model);
                }
                return RedirectToAction("ViewOrder",new { orderId = orderID, updated=true });
            }catch(Exception e)
            {
                return View("ErrorDisplay", new ErrorModel { Message = e.Message });
            }
        }

        public IActionResult ViewOrders(int pageNumber, int sortBy, bool successfullyDeleted = false)
        {
            var model = new OrdersViewModel();
            if (successfullyDeleted)
            {
                model.SuccessfullyDeleted = true;
            }
            model.Customers = mapper.Map<List<CustomerVM>>(customerService.GetCustomers());
            var OrderPage = orderService.GetOrders(pageNumber, sortBy);

            model.Orders = mapper.Map<List<OrderVM>>(OrderPage.Orders);
            model.Next = IsRemainingOrder(pageNumber, OrderPage.OrdersCount, OrderPage.Skip);
            model.SortBy = sortBy;
            model.PageNumber = pageNumber;
            return View(model);
        }

        private bool IsRemainingOrder(int pageNumber,int totalOrders,int skipCount)
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

        public IActionResult ViewOrdersForCustomer(int customerId, bool addedSuccessfully = false, bool updated=false)
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
            if (updated)
            {
                model.SuccessfullyUpdated = true;
            }
            model.SelectedCustomerName = mapper.Map<CustomerVM>(customerService.GetCustomerById(customerId)).Name;
            model.Orders = mapper.Map<List<OrderVM>>(orderService.GetOrdersForCustomer(customerId));
            model.Customers = mapper.Map<List<CustomerVM>>(customerService.GetCustomers());
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
            var model = new OrderVM();
            model.Id = orderId;
            return View(model);
        }
        [HttpPost]
        public IActionResult ConfirmDeleteOrder(OrderVM order)
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