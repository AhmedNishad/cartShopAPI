using Microsoft.EntityFrameworkCore;
using Shopping.Data.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Shopping.Data.Access
{
    public class OrderRepository
    {
        private CustomerRepository customerRepository { get; }
        private ProductRepository productRepository { get; }

        private readonly ShoppingContext db;

        public OrderRepository(ShoppingContext shoppingContext, ProductRepository productRepository, CustomerRepository customerRepository)
        {
            this.customerRepository = customerRepository;
            this.productRepository = productRepository;
            this.db = shoppingContext;
        }



        public List<OrderLineItemDO> AddOrder(int CustomerId, DateTime date, List<OrderLineItemDO> orderLineItems)
        {
            // Check if Order Present for same customer on same date
            bool orderExists = false;
            var existingOrder = new OrderDO();
            if (GetOrders().Exists(o => ((o.Customer.CustomerId == CustomerId) && (o.Date.ToShortDateString() == date.ToShortDateString()))))
            {
                orderExists = true;
                // If order exists get all existing line items
                existingOrder = db.Orders.Include(o => o.LineItems).ThenInclude(l => l.Product).FirstOrDefault(o => ((o.Date.ToShortDateString() == date.ToShortDateString()) && (o.Customer.CustomerId == CustomerId)));
            }


            var loopThroughItems = new List<OrderLineItemDO>();
            // create copy of list to solve collection has changed error
            foreach (var item in orderLineItems)
            {
                loopThroughItems.Add(item);
            }
            // List to store all the line items who'se quantity is too high
            var badItems = new List<OrderLineItemDO>();
            int lengthOfLoop = orderLineItems.Count;
            var order = new OrderDO();
            for (int i = 0; i < loopThroughItems.Count; i++)
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
            order.Customer = customerRepository.GetCustomerById(CustomerId);
            order.Date = date;
            order.Total = orderLineItems.Sum(l => l.Total);
            // Inititalize order to get ID
            if (orderLineItems.Count < 1)
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

        

        public int UpdateLineItem(OrderLineItemDO lineItem)
        {

            db.Update(lineItem);
            db.SaveChanges();
            return lineItem.OrderId;
        }

        public int UpdateLineItems(List<OrderLineItemDO> lineItems)
        {
            foreach (var item in lineItems)
            {
                db.OrderLineItems.Update(item);
                db.SaveChanges();
            }
            return lineItems[0].OrderId;
        }
        public int UpdateProductQuantity(int toReduce, int productId)
        {
            var product = db.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product.QuantityAtHand == 0)
            {
                return 0;
            }
            int remaining = product.QuantityAtHand - toReduce;
            // If there is less quantity than remaining, do not update
            if (remaining < 0)
            {
                return remaining;
            }
            product.QuantityAtHand = remaining;
            db.Products.Update(product);
            db.SaveChanges();
            return remaining;
        }

        public int UpdateProductQuantity(int toReduce, ProductDO product)
        {
            if (product.QuantityAtHand == 0)
            {
                return 0;
            }
            int remaining = product.QuantityAtHand - toReduce;
            // If there is less quantity than remaining, do not update
            if (remaining < 0)
            {
                return remaining;
            }
            product.QuantityAtHand = remaining;
            db.Products.Update(product);
            return remaining;
        }
        public void UpdateOrder(OrderDO updatedOrder)
        {
            var orderItems = updatedOrder.LineItems;
            var existingLineItems = new List<OrderLineItemDO>();
            for (int i = 0; i < orderItems.Count; i++)
            {
                // Collect all line items of the order together ( Append Quantities )
                var item = orderItems[i];
                if (existingLineItems.Exists(l => l.Product.ProductId == item.Product.ProductId))
                {
                    var existingItem = existingLineItems.FirstOrDefault(l => l.Product.ProductId == item.Product.ProductId);
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
           // db.Orders.Update(updatedOrder);
            db.SaveChanges();
        }

        public List<OrderLineItemDO> GetLineItemsForOrder(int OrderId)
        {
            var orders = db.OrderLineItems.Include(l => l.Product).Where(l => l.OrderId == OrderId).ToList();
            return orders;
        }

        public List<OrderDO> GetOrders()
        {
            return db.Orders.Include(o => o.Customer).OrderByDescending(o => o.Date).ToList();
        }

        public OrderDO GetOrderById(int OrderId)
        {

            return db.Orders.Include(o => o.Customer).Include(o => o.LineItems).FirstOrDefault(o => o.OrderId == OrderId);
        }
        public OrderLineItemDO GetLineItemById(int lineId)
        {

            return db.OrderLineItems.Include(o => o.Product).FirstOrDefault(o => o.LineId == lineId);
        }

        public List<OrderDO> GetOrdersForCustomer(int customerId)
        {
            return db.Orders.OrderByDescending(o => o.Date).Where(o => o.Customer.CustomerId == customerId).ToList();
        }


        public int DeleteLineItemById(int lineId)
        {
            var item = db.OrderLineItems.Include(l => l.Product).Include(l => l.Order).FirstOrDefault(l => l.LineId == lineId);

            var order = item.Order;
            order.LineItems = db.OrderLineItems.Include(l => l.Product).Where(l => l.OrderId == order.OrderId).ToList();
            // If only one line item remains for order. Do not process
            if (order.LineItems.Count == 1)
            {
                return 0;
            }
            if (item == null)
            {
                return 0;
            }
            productRepository.QuantityUpdateForProduct(item.Product.ProductId, item.Product.QuantityAtHand + item.Quantity);
            order.LineItems.Remove(item);
            order.Total = item.Order.LineItems.Sum(l => l.Total);
            db.SaveChanges();
            UpdateOrder(order);
            return 1;
        }

        public int DeleteOrder(int orderId)
        {
            var deleteingOrder = db.Orders.Include(o => o.LineItems).ThenInclude(l => l.Product).FirstOrDefault(o => o.OrderId == orderId);
            if (deleteingOrder == null)
            {
                return 0;
            }
            // Update Product quantities
            foreach (var lineItem in deleteingOrder.LineItems)
            {
                int updatedQuantity = lineItem.Product.QuantityAtHand + lineItem.Quantity;
                productRepository.QuantityUpdateForProduct(lineItem.Product.ProductId, updatedQuantity);
            }
            db.Orders.Remove(deleteingOrder);
            db.SaveChanges();
            return 1;
        }
    }
}
