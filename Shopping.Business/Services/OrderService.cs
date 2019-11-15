using AutoMapper;
using Shopping.Business.Entities;
using Shopping.Business.Exceptions;
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
        private IProductRepository productRepository { get; }
        private readonly IMapper mapper;
        public int skipCount = 5;

        public OrderService(IOrderRepository orderRepository,IProductRepository productRepository,
            ICustomerRepository customerRepository, IMapper mapper)
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

        public OrderPage GetOrders(int page, string sort="Latest")
        {
            var Page = new OrderPage();
            var sortedOrders = repository.GetOrders();
            Page.OrdersCount = sortedOrders.ToList().Count;
            Page.Skip = skipCount;
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
            // Retrieve the orders according to sorting logic
            var paginatedOrders = sortedOrders.Skip(page * skipCount).Take(skipCount).ToList();
            Page.Orders = mapper.Map<List<OrderBO>>(paginatedOrders);
            return Page;
        }
        

        public List<OrderBO> GetOrdersForCustomer(int customerId)
        {
            return mapper.Map<List<OrderBO>>(repository.GetOrdersForCustomer(customerId));
        }

        public List<OrderLineItemBO> GetLineItemsForOrder(int orderId)
        {
            return mapper.Map<List<OrderLineItemBO>>(repository.GetLineItemsForOrder(orderId));
        }

        //// Unimplimented
        //private void UpdateOrder(OrderBO updatedOrder, List<OrderLineItemBO> UpdatedItems)
        //{
        //    var orderItems = UpdatedItems; 
        //    var existingLineItems = new List<OrderLineItemBO>();
        //    for (int i = 0; i < orderItems.Count; i++)
        //    {
        //        // Collect all line items of the order together ( Append Quantities )
        //        var item = orderItems[i];
        //        if (existingLineItems.Exists(l => l.Product.Id == item.Product.Id))
        //        {
        //            // If item of same product already exists, remove it from list of orders
        //            var existingItem = existingLineItems.FirstOrDefault(l => l.Product.Id == item.Product.Id);
        //            existingItem.Quantity += item.Quantity;
        //            existingItem.Total += item.Total;
        //            orderItems.Remove(item);
        //        }
        //        else
        //        {
        //            existingLineItems.Add(item);
        //        }
        //    }
        //    updatedOrder.LineItems = existingLineItems;
        //    updatedOrder.Total = existingLineItems.Sum(l=> l.Total);
            
        //    repository.UpdateOrder(mapper.Map<OrderDO>(updatedOrder));
        //}

        private FilteredItems FilterLineItems(List<OrderLineItemBO> orderLineItems)
        {
            var filtered = new FilteredItems();
            var orderLineCopy = new List<OrderLineItemBO>();
            foreach (var item in orderLineItems)
            {
                orderLineCopy.Add(item);
            }
            var invalidItems = new List<OrderLineItemBO>();
            int ItemCount = orderLineItems.Count;
            for (int i = 0; i < orderLineCopy.Count; i++)
            {
                var item = orderLineCopy[i];
                int remaining = repository.UpdateProductQuantity(item.Quantity, item.Product.Id);
                if (remaining < 0)
                {
                    // Update the item quantity sent back to the page to whats remaining
                    item.Quantity = -1 * remaining;
                    invalidItems.Add(item);
                    orderLineItems.Remove(item);
                    continue;
                }
            }
            filtered.InvalidItems = invalidItems;
            filtered.ValidItems = orderLineItems;
            return filtered;
        }
        public List<OrderLineItemBO> AddOrder(int CustomerId, DateTime date, List<OrderLineItemBO> orderLineItems)
        {
            // Loop through entered line items and store all the line items who'se quantity is too high in a list with a 
            //      quantity of what is currently available
            var filteredItems = FilterLineItems(orderLineItems);
            var invalidItems = filteredItems.InvalidItems;
            orderLineItems = filteredItems.ValidItems;

            // If there isn't a single item in orderline items throw an inadequate quantity error
            if (orderLineItems.Count < 1)
            {
                throw new Exception($"There are inadequate products");
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
             // For order that doesn't exist add the line items after creating the order
            if (!orderExists)
            {
                var order = new OrderDO();
                order.Customer = customerRepository.GetCustomerById(CustomerId);
                order.Date = date;
                order.Total = orderLineItems.Sum(l => l.Total);
                int createdOrderId = repository.AddOrder(mapper.Map<OrderDO>(order));

                associatedOrderId = createdOrderId;
                order.OrderId = createdOrderId;
                repository.AddLineItemsForOrder(mapper.Map<List<OrderLineItemDO>>(orderLineItems),mapper
                    .Map<OrderDO>(order));
            }
            // For existing order. Add the order details to each line item and then update the order
            else
            {
                associatedOrderId = existingOrder.Id;
                var newListOfItems = existingOrder.LineItems;
                // repository.UpdateLineItemsForOrder(mapper.Map<List<OrderLineItemDO>>(orderLineItems), existingOrder.Id);
                foreach (var newItem in orderLineItems)
                {
                     newItem.Order = existingOrder;
                     newItem.OrderId = existingOrder.Id;
                     newListOfItems.Add(newItem);
                }
                existingOrder.LineItems = mapper.Map<List<OrderLineItemBO>>(newListOfItems);
                repository.UpdateLineItemsForOrder(mapper.Map<List<OrderLineItemDO>>(newListOfItems), associatedOrderId);
                repository.UpdateOrder(existingOrder.Id);
            }
            
            // Adds the created or existing orderId to end of the list being returned to controller
            invalidItems.Add(new OrderLineItemBO() { OrderId = associatedOrderId });
            return invalidItems;
        }

        // Unimplimented
        //public int UpdateLineItem(int lineId, OrderLineItemBO lineItem)
        //{
        //    var item = repository.GetLineItemById(lineId);
        //    // Get the difference in quantity of the item already present and new item
        //    int difference = lineItem.Quantity - item.Quantity;
        //    // Check if we have enough products
        //    int reduceResult = repository.UpdateProductQuantity(difference, lineItem.Product.Id);
        //    if (reduceResult < 0)
        //    {
        //        return reduceResult;
        //    }
        //    // Check if new item product is the same as existing
        //    if (lineItem.Product.Id != item.Product.ProductId)
        //    {
        //        item.Product = productRepository.GetProductById(lineItem.Product.Id);
        //    }
        //    item.Quantity = lineItem.Quantity;
        //    item.Total = lineItem.Total;
        //    repository.UpdateLineItem(item);
        //    // Update Product Quantity
        //    var updatedOrder = mapper.Map<OrderBO>(repository.GetOrderById(item.OrderId));

        //    updatedOrder.Total = GetLineItemsForOrder(item.OrderId).Sum(l => l.Total);
        //    // Ensure that the same product orders are aggregated
        //    //  UpdateOrder(updatedOrder);
        //    return updatedOrder.Id;
        //}

        public int DeleteOrder(int Id)
        {
            return repository.DeleteOrder(Id);
        }

        public void UpdateLineItemsForOrder(List<OrderLineItemBO> lineItems, int orderId)
        {
            var existingOrder = GetOrderById(orderId);
            var existingItems = existingOrder.LineItems;

            // Remove items that don't exist in the new order. Delete respective items
            for (int i = 0; i < existingItems.Count; i++ )
            {
                var oldItem = existingItems[i];
                if (!lineItems.Exists(l=> l.Id == oldItem.Id))
                {
                    existingOrder.LineItems.Remove(oldItem);
                    repository.DeleteLineItem(oldItem.Id);
                    // Add the quantity of the deleted line item
                    productRepository.QuantityUpdateForProduct(oldItem.Product.Id, 
                        oldItem.Product.QuantityAtHand + oldItem.Quantity);
                }
            }

            // Sort through and find the line items without an Id. To find the new Line items
            var newLineItems = new List<OrderLineItemBO>();
            for(int i = 0; i < lineItems.Count; i++)
            {
                var item = lineItems[i];
                if (item.Id == 0)
                {
                    var newProduct = mapper.Map<ProductBO>(productRepository.GetProductById(item.Product.Id));
                    var newItem = new OrderLineItemBO
                    {
                        Product = newProduct,
                        LinePrice = newProduct.UnitPrice,
                        Order = existingOrder,
                        OrderId = existingOrder.Id,
                        Quantity = item.Quantity
                    };
                    newLineItems.Add(newItem);
                    existingOrder.LineItems.Add(newItem);
                    // Deduct new product quantitiy
                    repository.UpdateProductQuantity(item.Quantity, item.Product.Id);
                }
                else
                {
                    var changedOrder = existingOrder.LineItems.FirstOrDefault(l => l.Id == item.Id);
                    // Add on difference between new product quantity and existing quantity
                    int remaining = repository.UpdateProductQuantity((item.Quantity - changedOrder.Quantity ), item.Product.Id);
                    if(remaining < 0)
                    {
                        throw new InadequateProductException((-1) * remaining);
                    }
                    changedOrder.Quantity = item.Quantity;
                }
            }

            // Add new Lineitems and Update existing items to db
            repository.UpdateLineItemsForOrder(mapper.Map<List<OrderLineItemDO>>(existingOrder.LineItems), existingOrder.Id);

            // Update the order properties and update in the db
            repository.UpdateOrder(existingOrder.Id);
        }
    }
}
