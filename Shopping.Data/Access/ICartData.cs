using Shopping.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.Data
{
    public interface ICartData
    {
        List<CustomerDO> GetCustomers();
        List<ProductDO> GetProducts();
        ProductDO GetProductById(int id);
        List<OrderLineItemDO> GetLineItemsForOrder(int OrderId);
        OrderDO GetOrderById(int OrderId);
        CustomerDO GetCustomerById(int customerId);
        List<OrderDO> GetOrdersForCustomer(int customerId);
        List<OrderDO> GetOrders();
        int UpdateLineItems(List<OrderLineItemDO> lineItems);
        int UpdateProductQuantity(int toReduce, int productId);
        int QuantityUpdateForProduct(int ProductId, int newQuantity);
        int UpdateLineItem(int lineId, OrderLineItemDO lineItem);
        int AddProduct(ProductDO product);
        int AddCustomer(CustomerDO customer);
        List<OrderLineItemDO> AddOrder(int CustomerId, DateTime date, List<OrderLineItemDO> orderLineItems);
        int deleteLineItemById(int orderLineItemId);
        int DeleteOrder(int orderId);
    }
}
