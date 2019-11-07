using Shopping.Business.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shopping.Business
{
    public interface IOrderService
    {
        OrderBO GetOrderById(int orderId);
        List<OrderLineItemBO> AddOrder(int customerId, DateTime date, List<OrderLineItemBO> orderLineItems);
        List<OrderBO> GetOrders(int page, string sort);
        bool AreThereRemainingOrders(int page);
        List<OrderBO> GetOrdersForCustomer(int customerId);
        List<OrderLineItemBO> GetLineItemsForOrder(int orderId);
        int UpdateLineItem(int lineId, OrderLineItemBO lineItem);
        int DeleteOrder(int Id);
        int DeleteLineItemById(int lineId);

    }
}
