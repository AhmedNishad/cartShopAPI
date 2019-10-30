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

        

        public List<OrderLineItem> AddOrder(int CustomerId, DateTime date, List<OrderLineItem> orderLineItems)
        {
            var loopThroughItems = new List<OrderLineItem>();
            foreach(var item in orderLineItems)
            {
                loopThroughItems.Add(item);
            }
            var badItems = new List<OrderLineItem>();
            int lengthOfLoop = orderLineItems.Count;
            var order = new Order();
            for(int i = 0; i < loopThroughItems.Count; i++)
            {
                var item = loopThroughItems[i];
                int remaining = UpdateProductQuantity(item.Quantity, item.Product.Id);
                if (remaining < 1)
                {
                    item.Quantity = -1 * remaining;
                    
                    badItems.Add(item);
                    orderLineItems.Remove(item);
                    continue;
                }
            }
            order.Customer = GetCustomerById(CustomerId);
            order.Date = date;
            order.Total = orderLineItems.Sum(l => l.Total);
            // Inititalize order to get ID
            if(orderLineItems.Count < 1)
            {
                return new List<OrderLineItem>();
            }
            db.Orders.Add(order);
            foreach (var item in orderLineItems) 
            {
                item.OrderId = order.Id;
                item.Order = order;
                
                db.OrderLineItems.Add(item);
                db.SaveChanges();
            }
            db.SaveChanges();
            badItems.Add(new OrderLineItem() { OrderId = order.Id });
            return badItems;
        }

        public int AddProduct(Product product)
        {
            db.Products.Add(product);
            db.SaveChanges();
            return product.Id;
        }

        // Unused
        public void CreateLineItemsForOrder(List<OrderLineItem> lineItems)
        {
            foreach(var item in lineItems)
            {
                try
                {
                    item.Product = GetProductById(item.Product.Id);
                }catch(Exception e)
                {
                    throw new Exception ("Product Not Found");
                }
                item.Total = item.Product.UnitPrice * item.Quantity;
                db.OrderLineItems.Add(item);
            }
            db.SaveChanges();
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
        public int UpdateProductQuantity(int toReduce, int productId)
        {
            var product = db.Products.FirstOrDefault(p => p.Id == productId);
            if(product.QuantityAtHand == 0)
            {
                return 0;
            }
            int remaining = product.QuantityAtHand - toReduce;
            if(remaining < 0)
            {
                return remaining;
            }
            product.QuantityAtHand = remaining;
            db.Products.Update(product);
            db.SaveChanges();
            return remaining;
        }

        public int QuantityUpdateForProduct(int ProductId, int newQuantity)
        {
            var product = db.Products.FirstOrDefault(p => p.Id == ProductId);
            product.QuantityAtHand = newQuantity;
            db.Products.Update(product);
            db.SaveChanges();
            return product.Id;
        }

        public int UpdateLineItem(int lineId, OrderLineItem lineItem)
        {
            var item = db.OrderLineItems.FirstOrDefault(l => l.Id == lineId);
            item.Product = lineItem.Product;
            // Get the difference in quantity of the item already present and new item
            int difference = lineItem.Quantity - item.Quantity;
            // Check if we have enough products
            int reduceResult = UpdateProductQuantity(difference, lineItem.Product.Id);
            if(reduceResult < 0)
            {
                return reduceResult;
            }
            item.Quantity = lineItem.Quantity;
            item.Total = lineItem.Total;
            db.OrderLineItems.Update(item);
            // Update Product Quantity
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
