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

        // --- Create Methods ---

        public List<OrderLineItem> AddOrder(int CustomerId, DateTime date, List<OrderLineItem> orderLineItems)
        {
            // Check if Order Present for same customer on same date
            bool orderExists = false;
            var existingOrder = new Order();
            if(GetOrders().Exists(o=> ((o.Customer.Id == CustomerId) && (o.Date.ToShortDateString() == date.ToShortDateString()))))
            {
                 orderExists = true;
                // If order exists get all existing line items
                 existingOrder = db.Orders.Include(o=> o.LineItems).ThenInclude(l=>l.Product).FirstOrDefault(o => ((o.Date.ToShortDateString() == date.ToShortDateString()) && (o.Customer.Id == CustomerId)));
            }


            var loopThroughItems = new List<OrderLineItem>();
            // create copy of list to solve collection has changed error
            foreach(var item in orderLineItems)
            {
                loopThroughItems.Add(item);
            }
            // List to store all the line items who'se quantity is too high
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
            int associatedOrderId = 0;
            if (!orderExists)
            {
                db.Orders.Add(order);
                db.SaveChanges();

                associatedOrderId = order.Id;
                foreach (var item in orderLineItems)
                {
                    item.OrderId = associatedOrderId;
                    item.Order = order;

                    db.OrderLineItems.Add(item);
                    db.SaveChanges();
                }
            }
            else
            {
                // Total to be managed
                associatedOrderId = existingOrder.Id;
                foreach (var item in orderLineItems) 
                {
                    item.OrderId = associatedOrderId;
                    item.Order = existingOrder;
                
                    db.OrderLineItems.Add(item);
                    db.SaveChanges();
                }
                existingOrder.Total = existingOrder.LineItems.Sum(o => o.Total);
                UpdateOrder(existingOrder);
            }
            badItems.Add(new OrderLineItem() { OrderId = associatedOrderId });
            return badItems;
        }

        public int AddProduct(Product product)
        {
            // Check if product of same name exists
            if(db.Products.ToList().Exists(p => p.ProductName.ToLower() == product.ProductName.ToLower()))
            {
                return db.Products.FirstOrDefault(p => p.ProductName.ToLower() == product.ProductName.ToLower()).Id;
            }
            db.Products.Add(product);
            db.SaveChanges();
            return product.Id;
        }

        public int AddCustomer(Customer customer)
        {
            // Check if customer of same name exists
            if (db.Customers.ToList().Exists(c => c.Name.ToLower() == customer.Name.ToLower()))
            {
                return 0;
            }
            db.Customers.Add(customer);
            db.SaveChanges();
            return customer.Id;
        }

        // --- Unused
        //public void CreateLineItemsForOrder(List<OrderLineItem> lineItems)
        //{
        //    foreach(var item in lineItems)
        //    {
        //        try
        //        {
        //            item.Product = GetProductById(item.Product.Id);
        //        }catch(Exception e)
        //        {
        //            throw new Exception ("Product Not Found");
        //        }
        //        item.Total = item.Product.UnitPrice * item.Quantity;
        //        db.OrderLineItems.Add(item);
        //    }
        //    db.SaveChanges();
        //}

        // --- Update Methods ---

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
            // If there is less quantity than remaining, do not update
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
            // Ensure that the same product orders are aggregated
            UpdateOrder(updatedOrder);
            db.SaveChanges();
            return item.OrderId;
        }

        private void UpdateOrder(Order updatedOrder)
        {
            var orderItems = updatedOrder.LineItems;
            var existingLineItems = new List<OrderLineItem>();
            for (int i = 0; i < orderItems.Count; i++)
            {
                // Collect all line items of the order together ( Append Quantities )
                var item = orderItems[i];
                if(existingLineItems.Exists(l => l.Product.Id == item.Product.Id))
                {
                    var existingItem = existingLineItems.FirstOrDefault(l => l.Product.Id == item.Product.Id);
                    existingItem.Quantity += item.Quantity;
                    existingItem.Total += item.Total;
                    orderItems.Remove(item);
                }
                else
                {
                    existingLineItems.Add(item);
                }
            }
            updatedOrder.LineItems = existingLineItems;
            db.Orders.Update(updatedOrder);
            db.SaveChanges();
        }

        // --- Update Methods ---

        public List<Customer> GetCustomers()
        { 
            return db.Customers.OrderBy(c=> c.Name).ToList();
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

        public List<Order> GetOrdersForCustomer(int customerId)
        {
            return db.Orders.OrderByDescending(o=> o.Date).Where(o => o.Customer.Id == customerId).ToList();
        }

        public Customer GetCustomerById(int CustomerId)
        {
            return db.Customers.FirstOrDefault(c => c.Id == CustomerId);
        }

        public List<Product> GetProducts()
        {
            return db.Products.OrderBy(p=> p.ProductName).ToList();
        }

        // --- Delete Methods

        public int deleteLineItemById(int lineId)
        {
            var item = db.OrderLineItems.Include(l=> l.Product).Include(l=> l.Order).FirstOrDefault(l => l.Id == lineId);
            
            var order = item.Order;
            order.LineItems = db.OrderLineItems.Include(l => l.Product).Where(l => l.OrderId == order.Id).ToList();
            // If only one line item remains for order. Do not process
            if(order.LineItems.Count == 1)
            {
                return 0;
            }
            if(item == null)
            {
                return 0;
            }
            QuantityUpdateForProduct(item.Product.Id, item.Product.QuantityAtHand + item.Quantity);
            order.LineItems.Remove(item);
            order.Total = item.Order.LineItems.Sum(l => l.Total);
            db.SaveChanges();
            UpdateOrder(order);
            return 1;
        }

        public int DeleteOrder(int orderId)
        {
            var deleteingOrder = db.Orders.Include(o=>o.LineItems).ThenInclude(l=>l.Product).FirstOrDefault(o => o.Id == orderId);
            if(deleteingOrder == null)
            {
                return 0;
            }
            // Update Product quantities
            foreach(var lineItem in deleteingOrder.LineItems)
            {
                int updatedQuantity = lineItem.Product.QuantityAtHand + lineItem.Quantity;
                QuantityUpdateForProduct(lineItem.Product.Id, updatedQuantity);
            }
            db.Orders.Remove(deleteingOrder);
            db.SaveChanges();
            return 1;
        }

        
    }
}
