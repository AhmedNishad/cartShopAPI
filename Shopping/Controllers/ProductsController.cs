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
    public class ProductsController : Controller
    {
        private ProductService productService { get; }
        public IMapper mapper { get; }

        public ProductsController(ProductService productService, IMapper mapper)
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
            model.Products = mapper.Map<List<Product>>(productService.GetProducts());
            return View(model);
        }

        [HttpGet]
        public IActionResult ViewProduct(int productId)
        {
            var model = new Product();
            model = mapper.Map<Product>(productService.GetProductById(productId));
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
            int result = productService.QuantityUpdateForProduct(product.Id, product.QuantityAtHand);
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
            int result = productService.AddProduct(mapper.Map<ProductBO>(product));
            return RedirectToAction("ViewProduct", new { productId = result });
        }
    }
}