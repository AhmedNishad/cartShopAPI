using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shopping.Data.Entities;
//using Shopping.Models;

namespace Shopping.Data
{
    public class CartData : ICartData
    {
        private readonly ShoppingContext db;

        public CartData(ShoppingContext context)
        {
            this.db = context;
        }

        // --- Create Methods ---

        public List<OrderLineItemDO> AddOrder(int CustomerId, DateTime date, List<OrderLineItemDO> orderLineItems)
        {
            // Check if Order Present for same customer on same date
            bool orderExists = false;
            var existingOrder = new OrderDO();
            if(GetOrders().Exists(o=> ((o.Customer.CustomerId == CustomerId) && (o.Date.ToShortDateString() == date.ToShortDateString()))))
            {
                 orderExists = true;
                // If order exists get all existing line items
                 existingOrder = db.Orders.Include(o=> o.LineItems).ThenInclude(l=>l.Product).FirstOrDefault(o => ((o.Date.ToShortDateString() == date.ToShortDateString()) && (o.Customer.CustomerId == CustomerId)));
            }


            var loopThroughItems = new List<OrderLineItemDO>();
            // create copy of list to solve collection has changed error
            foreach(var item in orderLineItems)
            {
                loopThroughItems.Add(item);
            }
            // List to store all the line items who'se quantity is too high
            var badItems = new List<OrderLineItemDO>();
            int lengthOfLoop = orderLineItems.Count;
            var order = new OrderDO();
            for(int i = 0; i < loopThroughItems.Count; i++)
            {
                var item = loopThroughItems[i];
                int remaining = UpdateProductQuantity(item.Quantity, item.Product.ProductId);
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
                return new List<OrderLineItemDO>();
            }
            int associatedOrderId = 0;
            if (!orderExists)
            {
                db.Orders.Add(order);
                db.SaveChanges();

                associatedOrderId = order.OrderId;
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
                associatedOrderId = existingOrder.OrderId;
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
            badItems.Add(new OrderLineItemDO() { OrderId = associatedOrderId });
            return badItems;
        }

        public int AddProduct(ProductDO product)
        {
            // Check if product of same name exists
            if(db.Products.ToList().Exists(p => p.ProductName.ToLower() == product.ProductName.ToLower()))
            {
                return db.Products.FirstOrDefault(p => p.ProductName.ToLower() == product.ProductName.ToLower()).ProductId;
            }
            db.Products.Add(product);
            db.SaveChanges();
            return product.ProductId;
        }

        public int AddCustomer(CustomerDO customer)
        {
            // Check if customer of same name exists
            if (db.Customers.ToList().Exists(c => c.Name.ToLower() == customer.Name.ToLower()))
            {
                return 0;
            }
            db.Customers.Add(customer);
            db.SaveChanges();
            return customer.CustomerId;
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

        public int UpdateLineItems(List<OrderLineItemDO> lineItems)
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
            var product = db.Products.FirstOrDefault(p => p.ProductId == productId);
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
            var product = db.Products.FirstOrDefault(p => p.ProductId == ProductId);
            product.QuantityAtHand = newQuantity;
            db.Products.Update(product);
            db.SaveChanges();
            return product.ProductId;
        }

        public int UpdateLineItem(int lineId, OrderLineItemDO lineItem)
        {
            var item = db.OrderLineItems.FirstOrDefault(l => l.LineId == lineId);
            item.Product = lineItem.Product;
            // Get the difference in quantity of the item already present and new item
            int difference = lineItem.Quantity - item.Quantity;
            // Check if we have enough products
            int reduceResult = UpdateProductQuantity(difference, lineItem.Product.ProductId);
            if(reduceResult < 0)
            {
                return reduceResult;
            }
            item.Quantity = lineItem.Quantity;
            db.OrderLineItems.Update(item);
            // Update Product Quantity
            var updatedOrder = db.Orders.FirstOrDefault(o => o.OrderId == item.OrderId);
            updatedOrder.Total = GetLineItemsForOrder(item.OrderId).Sum(l => l.Total);
            // Ensure that the same product orders are aggregated
            UpdateOrder(updatedOrder);
            db.SaveChanges();
            return item.OrderId;
        }

        private void UpdateOrder(OrderDO updatedOrder)
        {
            var orderItems = updatedOrder.LineItems;
            var existingLineItems = new List<OrderLineItemDO>();
            for (int i = 0; i < orderItems.Count; i++)
            {
                // Collect all line items of the order together ( Append Quantities )
                var item = orderItems[i];
                if(existingLineItems.Exists(l => l.Product.ProductId == item.Product.ProductId))
                {
                    var existingItem = existingLineItems.FirstOrDefault(l => l.Product.ProductId == item.Product.ProductId);
                    existingItem.Quantity += item.Quantity;
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

        public List<CustomerDO> GetCustomers()
        { 
            return db.Customers.OrderBy(c=> c.Name).ToList();
        }

        

        public ProductDO GetProductById(int id)
        {
            return db.Products.FirstOrDefault(p => p.ProductId == id);
        }

        public List<OrderLineItemDO> GetLineItemsForOrder(int OrderId)
        {
            return db.OrderLineItems.Include(l=>l.Product).Where(l => l.OrderId == OrderId).ToList();
        }

        public List<OrderDO> GetOrders()
        {
            return db.Orders.Include(o => o.Customer).OrderByDescending(o=> o.Date).ToList();
        }

        public OrderDO GetOrderById(int OrderId)
        {

            return db.Orders.Include(o => o.Customer).Include(o=>o.LineItems).FirstOrDefault(o => o.OrderId == OrderId);
        }

        public List<OrderDO> GetOrdersForCustomer(int customerId)
        {
            return db.Orders.OrderByDescending(o=> o.Date).Where(o => o.Customer.CustomerId == customerId).ToList();
        }

        public CustomerDO GetCustomerById(int CustomerId)
        {
            return db.Customers.FirstOrDefault(c => c.CustomerId == CustomerId);
        }

        public List<ProductDO> GetProducts()
        {
            return db.Products.OrderBy(p=> p.ProductName).ToList();
        }

        // --- Delete Methods

        public int deleteLineItemById(int lineId)
        {
            var item = db.OrderLineItems.Include(l=> l.Product).Include(l=> l.Order).FirstOrDefault(l => l.LineId == lineId);
            
            var order = item.Order;
            order.LineItems = db.OrderLineItems.Include(l => l.Product).Where(l => l.OrderId == order.OrderId).ToList();
            // If only one line item remains for order. Do not process
            if(order.LineItems.Count == 1)
            {
                return 0;
            }
            if(item == null)
            {
                return 0;
            }
            QuantityUpdateForProduct(item.Product.ProductId, item.Product.QuantityAtHand + item.Quantity);
            order.LineItems.Remove(item);
            order.Total = item.Order.LineItems.Sum(l => l.Total);
            db.SaveChanges();
            UpdateOrder(order);
            return 1;
        }

        public int DeleteOrder(int orderId)
        {
            var deleteingOrder = db.Orders.Include(o=>o.LineItems).ThenInclude(l=>l.Product).FirstOrDefault(o => o.OrderId == orderId);
            if(deleteingOrder == null)
            {
                return 0;
            }
            // Update Product quantities
            foreach(var lineItem in deleteingOrder.LineItems)
            {
                int updatedQuantity = lineItem.Product.QuantityAtHand + lineItem.Quantity;
                QuantityUpdateForProduct(lineItem.Product.ProductId, updatedQuantity);
            }
            db.Orders.Remove(deleteingOrder);
            db.SaveChanges();
            return 1;
        }

        
    }
}
