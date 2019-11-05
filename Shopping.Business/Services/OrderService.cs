using AutoMapper;
using Shopping.Business.Entities;
using Shopping.Data.Access;
using Shopping.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shopping.Business.Services
{
    public class OrderService
    {
        private readonly OrderRepository repository;
        private readonly IMapper mapper;

        private ProductRepository productRepository { get; }

        public OrderService(OrderRepository orderRepository,ProductRepository productRepository, IMapper mapper)
        {
            
            this.repository = orderRepository;
            this.productRepository = productRepository;
            this.mapper = mapper;
        }
        public OrderBO GetOrderById(int orderId)
        {
            var entity = repository.GetOrderById(orderId);
            return mapper.Map<OrderBO>(entity);
        }

        public List<OrderBO> GetOrders()
        {
            return mapper.Map<List<OrderBO>>(repository.GetOrders());
        }

        public List<OrderBO> GetOrdersForCustomer(int customerId)
        {
            return mapper.Map<List<OrderBO>>(repository.GetOrdersForCustomer(customerId));
        }

        public List<OrderLineItemBO> GetLineItemsForOrder(int orderId)
        {
            return mapper.Map<List<OrderLineItemBO>>(repository.GetLineItemsForOrder(orderId));
        }

        public List<OrderLineItemBO> AddOrder(int CustomerId, DateTime date, List<OrderLineItemBO> orderLineItems)
        {
            return mapper.Map<List<OrderLineItemBO>>(repository.AddOrder(CustomerId, date, mapper.Map<List<OrderLineItemDO>>(orderLineItems)));
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
            var updatedOrder = repository.GetOrderById(item.OrderId);

            updatedOrder.Total = GetLineItemsForOrder(item.OrderId).Sum(l => l.Total);
            // Ensure that the same product orders are aggregated
            repository.UpdateOrder(updatedOrder);
            return updatedOrder.OrderId;
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
