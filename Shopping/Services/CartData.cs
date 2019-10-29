using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shopping.Entities;
using Shopping.Models;

namespace Shopping.Services
{
    public class CartData : ICartData
    {
        private readonly ShoppingContext db;

        public CartData(ShoppingContext db)
        {
            this.db = db;
        }

        

        public int AddOrder(int CustomerId, DateTime date, List<OrderLineItem> orderLineItems)
        {
            var order = new Order();
            order.Customer = GetCustomerById(CustomerId);
            order.Date = date;
            order.Total = orderLineItems.Sum(l => l.Total);
            db.Orders.Add(order);
            db.SaveChanges();
            foreach (var item in orderLineItems) 
            {
                item.Product = GetProductById(item.Product.Id);
                item.OrderId = order.Id;
                item.Order = order;
                int remaining = UpdateProductQuantity(item.Quantity, item.Product.Id);
                if(remaining == 0)
                {
                    return 0;
                }
                item.Total = item.Product.UnitPrice * item.Quantity;
                db.OrderLineItems.Add(item);
                db.SaveChanges();
            }
            return order.Id;
        }

        public void CreateLineItemsForOrder(List<OrderLineItem> lineItems)
        {
            foreach(var item in lineItems)
            {
                item.Product = GetProductById(item.Product.Id);
                item.Total = item.Product.UnitPrice * item.Quantity;
                db.OrderLineItems.Add(item);
            }
            db.SaveChanges();
        }

        public int UpdateProductQuantity(int toReduce, int productId)
        {
            var product = db.Products.FirstOrDefault(p => p.Id == productId);
            int remaining = product.QuantityAtHand - toReduce;
            if(remaining < 0)
            {
                throw new Exception("No products remaining");
            }
            product.QuantityAtHand = remaining;
            db.Products.Update(product);
            db.SaveChanges();
            return remaining;
        }

        public int UpdateLineItems(List<OrderLineItem> lineItems)
        {
            foreach(var item in lineItems)
            {
                db.OrderLineItems.Update(item);
                db.SaveChanges();
            }
            return lineItems[0].OrderId;
        }

        public int UpdateLineItem(int lineId, OrderLineItem lineItem)
        {
            var item = db.OrderLineItems.FirstOrDefault(l => l.Id == lineId);
            item.Product = lineItem.Product;
            item.Quantity = lineItem.Quantity;

            // Update Product Quantity
            int reduceResult = UpdateProductQuantity(item.Quantity, lineItem.Product.Id);
            if(reduceResult == 0)
            {
                throw new Exception("There are inadequate products ");
            }
            item.Total = lineItem.Total;
            db.OrderLineItems.Update(item);
            var updatedOrder = db.Orders.FirstOrDefault(o => o.Id == item.OrderId);
            updatedOrder.Total = GetLineItemsForOrder(item.OrderId).Sum(l => l.Total);
            db.Orders.Update(updatedOrder);
            db.SaveChanges();
            return item.OrderId;
        }

        public List<Customer> GetCustomers()
        { 
            return db.Customers.ToList();
        }

        public Product GetProductById(int id)
        {
            return db.Products.FirstOrDefault(p => p.Id == id);
        }

        public List<OrderLineItem> GetLineItemsForOrder(int OrderId)
        {
            return db.OrderLineItems.Include(l=>l.Product).Where(l => l.OrderId == OrderId).ToList();
        }

        public List<Order> GetOrders()
        {
            return db.Orders.Include(o => o.Customer).OrderByDescending(o=> o.Date).ToList();
        }

        public Order GetOrderById(int OrderId)
        {

            return db.Orders.Include(o => o.Customer).Include(o=>o.LineItems).FirstOrDefault(o => o.Id == OrderId);
        }

        public Customer GetCustomerById(int CustomerId)
        {
            return db.Customers.FirstOrDefault(c => c.Id == CustomerId);
        }

        public List<Product> GetProducts()
        {
            return db.Products.ToList();
        }
        
        
    }
}
