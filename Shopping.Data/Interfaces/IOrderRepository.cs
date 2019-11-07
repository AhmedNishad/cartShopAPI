using Shopping.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shopping.Data.Interfaces
{
    public interface IOrderRepository
    {
        List<OrderLineItemDO> AddOrder(int CustomerId, DateTime date, List<OrderLineItemDO> orderLineItems);
        int AddOrder(OrderDO order);
        OrderDO GetOrderForCustomerOnDate(int customerId, DateTime date);
        int UpdateLineItem(OrderLineItemDO lineItem);
        int UpdateLineItems(List<OrderLineItemDO> lineItems);
        int UpdateProductQuantity(int toReduce, int productId);
        int UpdateProductQuantity(int toReduce, ProductDO product);
        void UpdateOrder(OrderDO updatedOrder);
        void UpdateOrder(int updatedOrderId);
        List<OrderLineItemDO> GetLineItemsForOrder(int OrderId);
        IEnumerable<OrderDO> GetOrders();
        OrderDO GetOrderById(int OrderId);
        OrderLineItemDO GetLineItemById(int lineId);

        void AddLineItemsForOrder(List<OrderLineItemDO> orderLineItems, OrderDO order);
        void UpdateLineItemsForOrder(List<OrderLineItemDO> orderLineItems, int orderId);
        List<OrderDO> GetOrdersForCustomer(int customerId);
        int DeleteLineItem(int lineId);
        int DeleteLineItemById(int lineId);
        int DeleteOrder(int orderId);
    }
}
