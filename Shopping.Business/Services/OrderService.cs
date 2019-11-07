using AutoMapper;
using Shopping.Business.Entities;
using Shopping.Data.Access;
using Shopping.Data.Entities;
using Shopping.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Shopping.Business.Services
{
    public class OrderService: IOrderService
    {
        private readonly IOrderRepository repository;

        private ICustomerRepository customerRepository { get; }

        private readonly IMapper mapper;
        public int skipCount = 3;


        private IProductRepository productRepository { get; }

        public OrderService(IOrderRepository orderRepository,IProductRepository productRepository, ICustomerRepository customerRepository, IMapper mapper)
        {
            
            this.repository = orderRepository;
            this.customerRepository = customerRepository;
            this.productRepository = productRepository;
            this.mapper = mapper;
        }
        public OrderBO GetOrderById(int orderId)
        {
            var entity = repository.GetOrderById(orderId);
            return mapper.Map<OrderBO>(entity);
        }

        public List<OrderBO> GetOrders(int page, string sort="Latest")
        {
            var sortedOrders = repository.GetOrders();
            // Sorting logic
            switch (sort)
            {
                case "Customer":
                    sortedOrders = sortedOrders.OrderBy(o => o.Customer.Name);
                    break;
                case "Total":
                    sortedOrders = sortedOrders.OrderByDescending(o => o.Total);
                    break;
                default:
                    sortedOrders = sortedOrders.OrderByDescending(o => o.Date);
                    break;
            }
            var paginatedOrders = sortedOrders.Skip(page * skipCount).Take(skipCount);
            return mapper.Map<List<OrderBO>>(paginatedOrders);
        }
        public bool AreThereRemainingOrders(int page)
        {
            // Get the next set of orders
            var nextOrders = repository.GetOrders().Skip((page + 1) * skipCount).Take(skipCount).ToArray();
            if (nextOrders.Length > 0)
            {
                return true;
            }
            return false;
        }

        public List<OrderBO> GetOrdersForCustomer(int customerId)
        {
            return mapper.Map<List<OrderBO>>(repository.GetOrdersForCustomer(customerId));
        }

        public List<OrderLineItemBO> GetLineItemsForOrder(int orderId)
        {
            return mapper.Map<List<OrderLineItemBO>>(repository.GetLineItemsForOrder(orderId));
        }

        private void UpdateOrder(OrderBO updatedOrder)
        {
            var missingItems = new List<OrderBO>();
            var orderItems = updatedOrder.LineItems; 
            var existingLineItems = new List<OrderLineItemBO>();
            for (int i = 0; i < orderItems.Count; i++)
            {
                // Collect all line items of the order together ( Append Quantities )
                var item = orderItems[i];
                if (existingLineItems.Exists(l => l.Product.Id == item.Product.Id))
                {
                    // If item of same product already exists, remove it from list of orders
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
            repository.UpdateLineItemsForOrder(mapper.Map<List<OrderLineItemDO>>(existingLineItems), updatedOrder.Id);
            updatedOrder.Total = existingLineItems.Sum(l=> l.Total);
          //  repository.AddLineItemsForOrder(mapper.Map<List<OrderLineItemDO>>(missingItems), mapper.Map<OrderDO>(updatedOrder));
            repository.UpdateOrder(mapper.Map<OrderDO>(updatedOrder));
        }

        public List<OrderLineItemBO> AddOrder(int CustomerId, DateTime date, List<OrderLineItemBO> orderLineItems)
        {
            // Loop through entered line items and store all the line items who'se quantity is too high in a list
            var loopThroughItems = new List<OrderLineItemBO>();
            foreach (var item in orderLineItems)
            {
                loopThroughItems.Add(item);
            }
            var invalidItems = new List<OrderLineItemBO>();
            int ItemCount = orderLineItems.Count;
            for (int i = 0; i < loopThroughItems.Count; i++)
            {
                var item = loopThroughItems[i];
                int remaining = repository.UpdateProductQuantity(item.Quantity, item.Product.Id);
                if (remaining < 1)
                {
                    item.Quantity = -1 * remaining;

                    invalidItems.Add(item);
                    orderLineItems.Remove(item);
                    continue;
                }
            }

            // If there isn't a single item in orderline items return empty list
            if (orderLineItems.Count < 1)
            {
                return new List<OrderLineItemBO>();
            }

            // Check if Order Present for same customer on same date
            bool orderExists = false;
            var existingOrder = new OrderBO();
            if (repository.GetOrders().ToList().Exists(o => ((o.Customer.CustomerId == CustomerId)
            && (o.Date.ToShortDateString() == date.ToShortDateString()))))
            {
                orderExists = true;
                // If order exists get existing Order
                existingOrder = mapper.Map<OrderBO>(repository.GetOrderForCustomerOnDate(CustomerId, date));
            }
            int associatedOrderId = 0;
            if (!orderExists)
            {
                // For order that doesn't exist add the line items after creating the order
                var order = new OrderDO();
                order.Customer = customerRepository.GetCustomerById(CustomerId);
                order.Date = date;
                order.Total = orderLineItems.Sum(l => l.Total);
                int createdOrderId = repository.AddOrder(mapper.Map<OrderDO>(order));

                associatedOrderId = createdOrderId;
                order.OrderId = createdOrderId;
                repository.AddLineItemsForOrder(mapper.Map<List<OrderLineItemDO>>(orderLineItems),mapper.Map<OrderDO>(order));
            }
            else
            {
                // For existing order. Add the order details to each line item and then update the order
                associatedOrderId = existingOrder.Id;
                // repository.UpdateLineItemsForOrder(mapper.Map<List<OrderLineItemDO>>(orderLineItems), existingOrder.Id);
                foreach (var newItem in orderLineItems)
                {
                     newItem.Order = existingOrder;
                     newItem.OrderId = existingOrder.Id;
                     existingOrder.LineItems.Add(newItem);
                }
                existingOrder.Total = existingOrder.LineItems.Sum(l => l.Total);
                
                UpdateOrder(existingOrder);
            }
            // Adds the created or existing orderId to end of the list being returned to controller
            invalidItems.Add(new OrderLineItemBO() { OrderId = associatedOrderId });
            return invalidItems;
        }

        public int UpdateLineItem(int lineId, OrderLineItemBO lineItem)
        {
            var item = repository.GetLineItemById(lineId);
            // Get the difference in quantity of the item already present and new item
            int difference = lineItem.Quantity - item.Quantity;
            // Check if we have enough products
            int reduceResult = repository.UpdateProductQuantity(difference, lineItem.Product.Id);
            if (reduceResult < 0)
            {
                return reduceResult;
            }
            // Check if new item product is the same as existing
            if (lineItem.Product.Id != item.Product.ProductId)
            {
                item.Product = productRepository.GetProductById(lineItem.Product.Id);
            }
            item.Quantity = lineItem.Quantity;
            item.Total = lineItem.Total;
            repository.UpdateLineItem(item);
            // Update Product Quantity
            var updatedOrder = mapper.Map<OrderBO>(repository.GetOrderById(item.OrderId));

            updatedOrder.Total = GetLineItemsForOrder(item.OrderId).Sum(l => l.Total);
            // Ensure that the same product orders are aggregated
            UpdateOrder(updatedOrder);
            return updatedOrder.Id;
        }

        public int DeleteOrder(int Id)
        {
            return repository.DeleteOrder(Id);
        }

        public int DeleteLineItemById(int lineId)
        {
            return repository.DeleteLineItemById(lineId);
        }
    }
}
