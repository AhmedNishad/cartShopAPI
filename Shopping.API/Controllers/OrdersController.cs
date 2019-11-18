using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shopping.Business;

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


    }
}