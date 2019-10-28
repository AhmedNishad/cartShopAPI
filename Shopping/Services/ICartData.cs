using Shopping.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.Services
{
    public interface ICartData
    {
        List<Customer> GetCustomers();
        List<Product> GetProducts();
        List<OrderLineItem> GetLineItemsForOrder(int OrderId);
        Order GetOrderById(int OrderId);
        int AddOrder(int CustomerId, DateTime date, List<OrderLineItem> orderLineItems);
    }
}
