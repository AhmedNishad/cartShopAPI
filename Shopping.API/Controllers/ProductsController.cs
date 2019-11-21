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
    public class ProductsController : ControllerBase
    {
        private IMapper mapper;
        private IProductService productService;
        public ProductsController(IMapper mapper, IProductService productService)
        {
            this.mapper = mapper;
            this.productService = productService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(productService.GetProducts());
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                return Ok(productService.GetProductById(id));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public IActionResult PostProduct(Product product)
        {
            try
            {
                int createdProductId = productService.AddProduct(mapper.Map<ProductBO>(product));
                return RedirectToAction("Get", new { id = createdProductId });
            } catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("/{id}/quantity")]
        public IActionResult UpdateProductQuantity(int id,[FromBody] Product UpdatedProduct)
        {
            if(UpdatedProduct.Id != id)
            {
                return BadRequest("Product ID does not match");
            }
            try
            {
                int updatedProductId = productService.QuantityUpdateForProduct(id, UpdatedProduct.QuantityAtHand);
                return RedirectToAction("Get", new { id = updatedProductId });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, [FromBody] Product UpdatedProduct)
        {
            if (UpdatedProduct.Id != id)
            {
                return BadRequest("Product ID does not match");
            }
            try
            {
                int updatedProductId = productService.UpdateProduct(id, mapper.Map<ProductBO>(UpdatedProduct));
                return RedirectToAction("Get", new { id = updatedProductId });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{id")]
        public IActionResult Delete(int id)
        {
            try
            {
                productService.DeleteProduct(id);
                return Ok("Successfully delete product");
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}