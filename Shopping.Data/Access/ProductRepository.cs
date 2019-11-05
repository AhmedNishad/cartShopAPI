using Microsoft.EntityFrameworkCore;
using Shopping.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shopping.Data.Access
{
    public class ProductRepository
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
                return db.Products.FirstOrDefault(p => p.ProductName.ToLower() == product.ProductName.ToLower()).ProductId;
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
            return db.Products.FirstOrDefault(p => p.ProductId == id);
        }

        public int QuantityUpdateForProduct(int ProductId, int newQuantity)
        {
            var product = db.Products.FirstOrDefault(p => p.ProductId == ProductId);
            product.QuantityAtHand = newQuantity;
            db.Products.Update(product);
            db.SaveChanges();
            return product.ProductId;
        }
    }
}
