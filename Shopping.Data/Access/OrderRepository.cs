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



        public List<OrderLineItemDO> AddOrder(int CustomerId, DateTime date, List<OrderLineItemDO> orderLineItems)
        {
            throw new NotImplementedException();
        }

        public OrderDO GetOrderForCustomerOnDate(int customerId, DateTime date)
        {
            return db.Orders.AsNoTracking().Include(o => o.LineItems).ThenInclude(l => l.Product).FirstOrDefault(o => 
            ((o.Date.ToShortDateString() == date.ToShortDateString()) && 
            (o.Customer.CustomerId == customerId)));
        }

        public int UpdateLineItem(OrderLineItemDO lineItem)
        {

            db.Update(lineItem);
            db.SaveChanges();
            return lineItem.OrderId;
        }

        public int UpdateLineItems(List<OrderLineItemDO> lineItems)
        {
            var existingOrder = db.Orders.FirstOrDefault(o => o.OrderId == lineItems[0].OrderId);
            foreach (var item in lineItems)
            {
                db.OrderLineItems.Update(item);
            }
            existingOrder.Total = existingOrder.LineItems.Sum(o => o.Total);

            db.SaveChanges();
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
            var existingOrder = GetOrderById(updatedOrder.OrderId);
            //existingOrder.LineItems = updatedOrder.LineItems;
            existingOrder.Total = updatedOrder.Total;
            Debug.WriteLine(db.Entry(updatedOrder).State);
            db.Orders.Update(existingOrder);
            //db.Entry(updatedOrder).State = EntityState.Modified;
            db.SaveChanges();
        }
        public void UpdateOrder(int updatedOrderId)
        {
            var updatedOrder = db.Orders.Include(o => o.LineItems).FirstOrDefault(o => o.OrderId == updatedOrderId);
            var orderItems = updatedOrder.LineItems;
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

        public IEnumerable<OrderDO> GetOrders()
        {
            return db.Orders.Include(o => o.Customer);
        }

        public OrderDO GetOrderById(int OrderId)
        {

            return db.Orders.Include(o => o.Customer).Include(o => o.LineItems).FirstOrDefault(o => o.OrderId == OrderId);
        }
        public OrderLineItemDO GetLineItemById(int lineId)
        {

            return db.OrderLineItems.Include(o => o.Product).FirstOrDefault(o => o.LineId == lineId);
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

        // Redundant
        public void UpdateLineItemsForOrder(List<OrderLineItemDO> orderLineItems, int orderId)
        {
            foreach (var item in orderLineItems)
            {
                //db.OrderLineItems.Update(item);
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
            // Update quantity of product
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
