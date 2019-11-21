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

        public OrderPage GetOrders(int page, int sort=0)
        {
            var Page = new OrderPage();
            var sortedOrders = repository.GetOrders();
            Page.OrdersCount = sortedOrders.ToList().Count;
            Page.Skip = skipCount;
            // Sorting logic
            switch (sort)
            {
                case 1:
                    sortedOrders = sortedOrders.OrderBy(o => o.Customer.Name);
                    break;
                case 2:
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

        private FilteredItems ValidateLineItems(List<OrderLineItemBO> orderLineItems)
        {
            var orderLineCopy = new List<OrderLineItemBO>();
            foreach (var item in orderLineItems)
            {
                orderLineCopy.Add(item);
            }
            var invalidItems = new List<OrderLineItemBO>();
            for (int i = 0; i < orderLineCopy.Count; i++)
            {
                var item = orderLineCopy[i];
                var product = productRepository.GetProductById(item.Product.Id);
                // Check if there enough products remaining
                int quantityRemaining = product.QuantityAtHand - item.Quantity;
                if (quantityRemaining < 0)
                {
                    // Update the item quantity sent back to the page to what is remaining at hand
                    item.Quantity = -1 * quantityRemaining;
                    invalidItems.Add(item);
                    orderLineItems.Remove(item);
                    continue;
                }
            }
            var filtered = new FilteredItems();
            filtered.InvalidItems = invalidItems;
            filtered.ValidItems = orderLineItems;
            return filtered;
        }

        private OrderBO FindExistingOrder(int customerId, DateTime date)
        {
            OrderBO existingOrder= null;
            if (repository.GetOrders().ToList().Exists(o => ((o.Customer.CustomerId == customerId)
            && (o.Date.ToShortDateString() == date.ToShortDateString()))))
            {
                // If order exists get existing Order
                existingOrder = mapper.Map<OrderBO>(repository.GetOrderForCustomerOnDate(customerId, date));
            }
            return existingOrder;
        }
       
        /// <summary>
        ///  Creates a new order for a given customer on a given date. 
        ///  Updates the product quantities of valid items
        ///  If Order exists for the same date, appends the product quantities
        ///  and updates the existing order
        ///  { Throws error when none of the items are valid }
        /// </summary>
        /// <param name="CustomerId"></param>
        /// <param name="date"></param>
        /// <param name="orderLineItems"></param>
        /// <returns> List of Rejected Line Items (Quantity Too High) </returns>
        public OrderAddedResult AddOrder(int CustomerId, DateTime date, List<OrderLineItemBO> orderLineItems)
        {
            var addedResult = new OrderAddedResult();
            bool isUpdated = false;
            //      Step 1 : Validate Line items
            var validatedItems = ValidateLineItems(orderLineItems);
           
            // Loop through entered line items and store all the line items who'se quantity is too high in a list with a 
            //      quantity of what is currently available

            var invalidItems = validatedItems.InvalidItems;
            orderLineItems = validatedItems.ValidItems;

            // Update Product Quantities
            foreach(var item in orderLineItems)
            {
                repository.UpdateProductQuantity(item.Quantity, item.Product.Id);
            }

            // If there isn't a single item in orderline items throw an inadequate quantity error
            if (orderLineItems.Count < 1)
            {
                throw new Exception($"There are inadequate products");
            }

            //       Step 2 : Find Existing Order ( If Exists )

            // Check if Order Present for same customer on same date
            var existingOrder = FindExistingOrder(CustomerId, date);
            
             // For order that doesn't exist add the line items after creating the order
            if (existingOrder == null)
            {
                var order = new OrderDO();
                order.Customer = customerRepository.GetCustomerById(CustomerId);
                order.Date = date;
                order.Total = orderLineItems.Sum(l => l.Total);
                int createdOrderId = repository.AddOrder(mapper.Map<OrderDO>(order));

                order.OrderId = createdOrderId;
                repository.AddLineItemsForOrder(mapper.Map<List<OrderLineItemDO>>(orderLineItems),mapper
                    .Map<OrderDO>(order));
            }
            // For existing order. Add the order details to each line item and then update the order
            else
            {
                var newListOfItems = existingOrder.LineItems;
                // repository.UpdateLineItemsForOrder(mapper.Map<List<OrderLineItemDO>>(orderLineItems), existingOrder.Id);
                foreach (var newItem in orderLineItems)
                {
                     newItem.Order = existingOrder;
                     newItem.OrderId = existingOrder.Id;
                     newListOfItems.Add(newItem);
                }
                existingOrder.LineItems = mapper.Map<List<OrderLineItemBO>>(newListOfItems);
                repository.UpdateLineItemsForOrder(mapper.Map<List<OrderLineItemDO>>(newListOfItems), existingOrder.Id);
                repository.UpdateOrder(existingOrder.Id);
                isUpdated = true;
            }
            
            //      Step 3 : Construct Response Object

            addedResult.InvalidItems = invalidItems;
            addedResult.Updated = isUpdated; 
            return addedResult;
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
            var deletingOrder = repository.GetOrderById(Id);
            // Update Product quantities
            foreach (var lineItem in deletingOrder.LineItems)
            {
                int updatedQuantity = lineItem.Product.QuantityAtHand + lineItem.Quantity;
                productRepository.QuantityUpdateForProduct(lineItem.Product.ProductId, updatedQuantity);
            }
            return repository.DeleteOrder(Id);
        }

        /// <summary>
        /// Updates the line item for the given order. Deletes the items that don't exist in the incoming list
        /// Adds the new line items to the order and updates the items that have been changed.
        /// </summary>
        /// <param name="lineItems">Updated Line Items from controller</param>
        /// <param name="orderId">Order ID Of order being updated</param>
        /// <returns>List of invalid items that are not valid (Quantity too high)</returns>
        public OrderUpdatedResult UpdateLineItemsForOrder(List<OrderLineItemBO> lineItems, int orderId)
        {
            var updatedResult = new OrderUpdatedResult();
            updatedResult.Date = repository.GetOrderById(orderId).Date;
            var validatedItems = ValidateLineItems(lineItems);

            lineItems = validatedItems.ValidItems;
            var existingOrder = GetOrderById(orderId);
            var existingItems = existingOrder.LineItems;

            //      Case 1 : Line items have been removed

            // Remove items that don't exist in the new order. Delete respective items
            for (int i = 0; i < existingItems.Count; i++)
            {
                var oldItem = existingItems[i];
                if (!lineItems.Exists(l => l.Id == oldItem.Id))
                {
                    existingOrder.LineItems.Remove(oldItem);
                    repository.DeleteLineItem(oldItem.Id);
                    // Add the quantity of the deleted line item
                    productRepository.QuantityUpdateForProduct(oldItem.Product.Id,
                        oldItem.Product.QuantityAtHand + oldItem.Quantity);
                }
            }

            //      Case 2 : New Line Items have been added

            // Sort through and find the line items without an Id. To find the new Line items
            var newLineItems = new List<OrderLineItemBO>();
            for (int i = 0; i < lineItems.Count; i++)
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
            }

            //      Case 3 : Existing Products have been Changed

            // Update the product quantities that have been added
            for(int i = 0; i < lineItems.Count; i++)
            {
                var item = lineItems[i];
                if(item.Id != 0)
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

            updatedResult.InvalidItems = validatedItems.InvalidItems;
            return updatedResult;
        }
    }
}
