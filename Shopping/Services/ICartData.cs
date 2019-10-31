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
        Product GetProductById(int id);
        List<OrderLineItem> GetLineItemsForOrder(int OrderId);
        Order GetOrderById(int OrderId);
        List<Order> GetOrdersForCustomer(int customerId);
        List<OrderLineItem> AddOrder(int CustomerId, DateTime date, List<OrderLineItem> orderLineItems);
        List<Order> GetOrders();
        int UpdateLineItems(List<OrderLineItem> lineItems);
        int UpdateLineItem(int lineId, OrderLineItem lineItem);
        int AddProduct(Product product);
        int AddCustomer(Customer customer);
        int UpdateProductQuantity(int toReduce, int productId);
        int QuantityUpdateForProduct(int ProductId, int newQuantity);
    }
}
