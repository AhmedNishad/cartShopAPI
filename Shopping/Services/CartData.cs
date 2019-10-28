using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shopping.Entities;
using Shopping.Models;

namespace Shopping.Services
{
    public class CartData : ICartData
    {
        private readonly ShoppingContext db;

        public CartData(ShoppingContext db)
        {
            this.db = db;
        }

        

        public int AddOrder(int CustomerId, DateTime date, List<OrderLineItem> orderLineItems)
        {
            var order = new Order();
            order.Customer = GetCustomerById(CustomerId);
            order.Date = date;
            CreateLineItemsForOrder(orderLineItems);
            db.Orders.Add(order);
            db.SaveChanges();
            return 1;
        }

        public void CreateLineItemsForOrder(List<OrderLineItem> lineItems)
        {
            foreach(var item in lineItems)
            {
                item.Product = GetProductById(item.Product.Id);
                item.Total = item.Product.UnitPrice * item.Quantity;
                db.OrderLineItems.Add(item);
            }
            db.SaveChanges();
        }

        public List<Customer> GetCustomers()
        { 
            return db.Customers.ToList();
        }

        public Product GetProductById(int id)
        {
            return db.Products.FirstOrDefault(p => p.Id == id);
        }

        public List<OrderLineItem> GetLineItemsForOrder(int OrderId)
        {
            throw new NotImplementedException();
        }

        public Order GetOrderById(int OrderId)
        {
            throw new NotImplementedException();
        }

        public Customer GetCustomerById(int CustomerId)
        {
            return db.Customers.FirstOrDefault(c => c.Id == CustomerId);
        }

        public List<Product> GetProducts()
        {
            return db.Products.ToList();
        }
    }
}
