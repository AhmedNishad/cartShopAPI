using Shopping.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shopping.Data.Interfaces
{
    public interface IOrderRepository
    {
        int AddOrder(OrderDO order);
        OrderDO GetOrderForCustomerOnDate(int customerId, DateTime date);
        int UpdateProductQuantity(int toReduce, int productId);
        void UpdateOrder(int updatedOrderId);
        List<OrderLineItemDO> GetLineItemsForOrder(int OrderId);
        IQueryable<OrderDO> GetOrders();
        OrderDO GetOrderById(int OrderId);
        OrderLineItemDO GetLineItemById(int lineId);

        void AddLineItemsForOrder(List<OrderLineItemDO> orderLineItems, OrderDO order);
        void UpdateLineItemsForOrder(List<OrderLineItemDO> orderLineItems, int orderId);
        List<OrderDO> GetOrdersForCustomer(int customerId);
        int DeleteLineItem(int lineId);
        int DeleteOrder(int orderId);
    }
}
