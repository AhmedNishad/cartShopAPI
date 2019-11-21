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
    public class OrdersController : ControllerBase
    {
        private IMapper mapper;
        private IOrderService orderService;
        public OrdersController(IOrderService orderService, IMapper mapper)
        {
            this.mapper = mapper;
            this.orderService = orderService;
        }

        [HttpGet("/all/{page}")]
        public IActionResult GetOrders(int page, [FromQuery] int sort)
        {
            return Ok(orderService.GetOrders(page, sort));
        }

        [HttpPost("{customerId}")]
        public IActionResult AddOrder(Order order, int customerId)
        {
            try
            {
                var orderAddedResult = orderService.AddOrder(customerId, order.Date, mapper.Map<List<OrderLineItemBO>>(order.LineItems));
                if(orderAddedResult.InvalidItems.Count > 0)
                {
                    return BadRequest(orderAddedResult.InvalidItems);
                }
                return RedirectToAction("GetOrdersForCustomer", new { customerId });
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateOrder(int id, Order updatedOrder)
        {
            if(id != updatedOrder.Id)
            {
                return BadRequest("Id does not match request");
            }
            try
            {
                var updatedResult = orderService.UpdateLineItemsForOrder(mapper.Map<List<OrderLineItemBO>>(updatedOrder.LineItems), id);
                if(updatedResult.InvalidItems.Count > 0)
                {
                    return BadRequest(updatedResult.InvalidItems);
                }
                return RedirectToAction("GetOrder", new { id });
            }catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetOrder(int id) 
        {
            return Ok(orderService.GetOrderById(id));
        }
        [HttpGet("/customer/{customerId}")]
        public IActionResult GetOrderForCustomer(int customerId)
        {
            return Ok(orderService.GetOrdersForCustomer(customerId));
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteOrder(int id)
        {
            try
            {
                orderService.DeleteOrder(id);
                return Ok("Customer successfully deleted");
            }catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}