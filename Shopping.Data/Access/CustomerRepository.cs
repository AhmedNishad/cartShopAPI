using Shopping.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shopping.Data.Access
{
    public class CustomerRepository
    {
        private readonly ShoppingContext db;

        public CustomerRepository(ShoppingContext context)
        {
            this.db = context;
        }

        public int AddCustomer(CustomerDO customer)
        {
            // Check if customer of same name exists
            if (db.Customers.ToList().Exists(c => c.Name.ToLower() == customer.Name.ToLower()))
            {
                return 0;
            }
            db.Customers.Add(customer);
            db.SaveChanges();
            return customer.CustomerId;
        }

        public List<CustomerDO> GetCustomers()
        {
            var customers = db.Customers.OrderBy(c => c.Name).ToList();
            return customers;
        }

        public CustomerDO GetCustomerById(int CustomerId)
        {
            return db.Customers.FirstOrDefault(c => c.CustomerId == CustomerId);
        }
    }
}
