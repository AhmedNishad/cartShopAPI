using Microsoft.EntityFrameworkCore;
using Shopping.Data.Entities;
using Shopping.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Shopping.Data.Access
{
    public class OrderRepository : IOrderRepository
    {
        private ICustomerRepository customerRepository { get; }
        private IProductRepository productRepository { get; }

        private readonly ShoppingContext db;

        public OrderRepository(ShoppingContext shoppingContext, IProductRepository productRepository, ICustomerRepository customerRepository)
        {
            this.customerRepository = customerRepository;
            this.productRepository = productRepository;
            this.db = shoppingContext;
        }


        public OrderDO GetOrderForCustomerOnDate(int customerId, DateTime date)
        {
            // No null check as is being checked in service method prior to getting
            return db.Orders.Include(o => o.LineItems).ThenInclude(l => l.Product).Include(o=>o.Customer)
                .AsNoTracking().FirstOrDefault(o => 
            ((o.Date.ToShortDateString() == date.ToShortDateString()) && 
            (o.Customer.CustomerId == customerId)));
        }

        // Unimplimented
        //public int UpdateLineItem(OrderLineItemDO lineItem)
        //{
        //    db.Update(lineItem);
        //    db.SaveChanges();
        //    return lineItem.OrderId;
        //}

        //public int UpdateLineItems(List<OrderLineItemDO> lineItems)
        //{
        //    var existingOrder = db.Orders.FirstOrDefault(o => o.OrderId == lineItems[0].OrderId);
        //    foreach (var item in lineItems)
        //    {
        //        db.OrderLineItems.Update(item);
        //    }
        //    existingOrder.Total = existingOrder.LineItems.Sum(o => o.Total);

        //    db.SaveChanges();
        //    return lineItems[0].OrderId;
        //}
        public int UpdateProductQuantity(int toReduce, int productId)
        {
            var product = db.Products.FirstOrDefault(p => p.ProductId == productId);
            // If there is less quantity than remaining or if product quantity at hand is 0, do not update
            if (product.QuantityAtHand == 0)
            {
                return 0;
            }
            int remaining = product.QuantityAtHand - toReduce;
            if (remaining < 0)
            {
                return remaining;
            }
            product.QuantityAtHand = remaining;
            db.SaveChanges();
            return remaining;
        }

        private int UpdateProductQuantity(int toReduce, ProductDO product)
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
            db.SaveChanges();
            return remaining;
        }
        private void UpdateOrder(OrderDO updatedOrder)
        {
            db.Orders.Update(updatedOrder);
            db.SaveChanges();
        }
        public void UpdateOrder(int updatedOrderId)
        {
            var updatedOrder = db.Orders.Include(o => o.LineItems)
                .FirstOrDefault(o => o.OrderId == updatedOrderId);
            // No null check as is already being checked in service methods
            
            updatedOrder.Total = updatedOrder.LineItems.Sum(l => l.Total);
            db.SaveChanges();
        }


        public List<OrderLineItemDO> GetLineItemsForOrder(int OrderId)
        {
            var orders = db.OrderLineItems.Include(l => l.Product).Where(l => l.OrderId == OrderId).ToList();
            return orders;
        }

        public IQueryable<OrderDO> GetOrders()
        {
            return db.Orders.Include(o => o.Customer).AsQueryable();
        }

        public OrderDO GetOrderById(int OrderId)
        {
            var order = db.Orders.Include(o => o.Customer).Include(o => o.LineItems).ThenInclude(l => l.Product)
                .AsNoTracking().FirstOrDefault(o => o.OrderId == OrderId);
            if(order == null)
            {
                throw new Exception($"Order does not exist for ID {OrderId}");
            }
            return order;
        }
        public OrderLineItemDO GetLineItemById(int lineId)
        {
            var lineItem = db.OrderLineItems.Include(o => o.Product).FirstOrDefault(o => o.LineId == lineId);
            if(lineItem == null)
            {
                throw new Exception($"Line item does not exist for line ID {lineId}");
            }
            return lineItem;
        }

        public int AddOrder(OrderDO order)
        {
            db.Orders.Add(order);
            db.SaveChanges();
            return order.OrderId;
        }

        public void AddLineItemsForOrder(List<OrderLineItemDO> orderLineItems, OrderDO order)
        {
            foreach (var item in orderLineItems)
            {
                item.OrderId = order.OrderId;
                item.Order = order;

                db.OrderLineItems.Add(item);
            }
            
            db.SaveChanges();
        }

        public void UpdateLineItemsForOrder(List<OrderLineItemDO> orderLineItems, int orderId)
        {
            var orderItems = orderLineItems;
            var existingLineItems = new List<OrderLineItemDO>();
            for (int i = 0; i < orderItems.Count; i++)
            {
                // Collect all line items of the order together ( Append Quantities )
                var item = orderItems[i];
                if (existingLineItems.Exists(l => l.Product.ProductId == item.Product.ProductId))
                {
                    // If item of same product already exists, remove it from list of orders
                    var existingItem = existingLineItems.FirstOrDefault(l => l.Product.ProductId == item.Product.ProductId);
                    existingItem.Quantity += item.Quantity;
                    orderItems.Remove(item);
                }
                else
                {
                    existingLineItems.Add(item);
                }
            }
            foreach (var item in existingLineItems)
            {
                if(item.LineId == 0) 
                {
                    db.Entry(item).State = EntityState.Added;
                }
                else
                {
                    db.Entry(item).State = EntityState.Modified;
                }
            }
            db.SaveChanges();
        }

        public List<OrderDO> GetOrdersForCustomer(int customerId)
        {
            return db.Orders.OrderByDescending(o => o.Date).Where(o => o.Customer.CustomerId == customerId).ToList();
        }

        public int DeleteLineItem(int lineId)
        {
            var item = db.OrderLineItems.FirstOrDefault(l => l.LineId == lineId);
            db.OrderLineItems.Remove(item);
            db.SaveChanges();
            return 1;
        }


        private int DeleteLineItemById(int lineId)
        {
            var item = db.OrderLineItems.Include(l => l.Product).Include(l => l.Order).AsNoTracking().FirstOrDefault(l => l.LineId == lineId);

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
            // Update quantity of product
            
            order.LineItems.Remove(item);
            order.Total = item.Order.LineItems.Sum(l => l.Total);
            UpdateOrder(order);
           // db.SaveChanges();
            return 1;
        }

        public int DeleteOrder(int orderId)
        {
            var deletingOrder = db.Orders.Include(o => o.LineItems).ThenInclude(l => l.Product)
                .FirstOrDefault(o => o.OrderId == orderId);
            if (deletingOrder == null)
            {
                return 0;
            }
            
            db.Orders.Remove(deletingOrder);
            db.SaveChanges();
            return 1;
        }
    }
}
