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
        OrderPage GetOrders(int page, string sort);
        List<OrderBO> GetOrdersForCustomer(int customerId);
        List<OrderLineItemBO> GetLineItemsForOrder(int orderId);
        int DeleteOrder(int Id);
        void UpdateLineItemsForOrder(List<OrderLineItemBO> lineItems, int orderId);
    }
}
