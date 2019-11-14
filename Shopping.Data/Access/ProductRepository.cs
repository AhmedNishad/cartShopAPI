using Microsoft.EntityFrameworkCore;
using Shopping.Data.Entities;
using Shopping.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shopping.Data.Access
{
    public class ProductRepository : IProductRepository
    {
        private readonly ShoppingContext db;

        public ProductRepository(ShoppingContext context)
        {
            this.db = context;
        }
        public int AddProduct(ProductDO product)
        {
            // Check if product of same name exists
            if (db.Products.ToList().Exists(p => p.ProductName.ToLower() == product.ProductName.ToLower()))
            {
                return db.Products.FirstOrDefault(p => p.ProductName.ToLower() == product.ProductName.ToLower())
                    .ProductId;
            }
            db.Products.Add(product);
            db.SaveChanges();
            return product.ProductId;
        }

        public List<ProductDO> GetProducts()
        {
            return db.Products.OrderBy(p => p.ProductName).ToList();
        }

        public ProductDO GetProductById(int id)
        {
            var product = db.Products.AsNoTracking().FirstOrDefault(p => p.ProductId == id);
            if (product == null)
            {
                throw new Exception($"Product does not exist for Id {id}");
            }
            return product;
        }

        public int QuantityUpdateForProduct(int ProductId, int newQuantity)
        {
            var product = db.Products.FirstOrDefault(p => p.ProductId == ProductId);
            if(product == null)
            {
                throw new Exception($"Product does not exist for Id {ProductId}");
            }
            product.QuantityAtHand = newQuantity;
            db.Products.Update(product);
            db.SaveChanges();
            return product.ProductId;
        }
    }
}
