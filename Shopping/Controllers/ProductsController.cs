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
    public class ProductsController : Controller
    {
        private IProductService productService { get; }
        public IMapper mapper { get; }

        public ProductsController(IProductService productService, IMapper mapper)
        {
            this.productService = productService;
            this.mapper = mapper;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ViewProducts()
        {
            var model = new ProductsViewModel();
            model.Products = mapper.Map<List<ProductVM>>(productService.GetProducts());
            return View(model);
        }

        [HttpGet]
        public IActionResult ViewProduct(int productId)
        {
            var model = new ProductVM();
            model = mapper.Map<ProductVM>(productService.GetProductById(productId));
            return View(model);
        }

        [HttpPost]
        public IActionResult UpdateProduct(ProductVM product)
        {
            if (!ModelState.IsValid)
            {
                product.QuantityAtHand = 0;
                return View(product);
            }
            try
            {
                int result = productService.QuantityUpdateForProduct(product.Id, product.QuantityAtHand);
                return RedirectToAction("ViewProduct", new { productId = result });
            }
            catch (Exception e)
            {
                return View("ErrorDisplay", new ErrorModel { Message = e.Message });
            }
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            var model = new ProductVM();

            return View(model);
        }

        [HttpPost]
        public IActionResult AddProduct(ProductVM product)
        {
            if (!ModelState.IsValid)
            {
                return View(product);
            }
            int result = productService.AddProduct(mapper.Map<ProductBO>(product));
            // If product exists. Redirect to existing product
            return RedirectToAction("ViewProduct", new { productId = result });
        }
    }
}