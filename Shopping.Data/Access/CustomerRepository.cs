using Shopping.Data.Entities;
using Shopping.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shopping.Data.Access
{
    public class CustomerRepository : ICustomerRepository
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
            var customer = db.Customers.FirstOrDefault(c => c.CustomerId == CustomerId);
            if(customer == null)
            {
                throw new Exception($"Customer of ID {CustomerId} does not exist");
            }
            return customer;
        }

        public void DeleteCustomerById(int id)
        {
            var customerToRemove = db.Customers.FirstOrDefault(c => c.CustomerId == id);
            if(customerToRemove == null)
            {
                throw new Exception($"Customer of id {id} does not exist");
            }
            db.Customers.Remove(customerToRemove);
            db.SaveChanges();
        }

        public void UpdateCustomer(CustomerDO customer)
        {
            var customerToUpdate = db.Customers.FirstOrDefault(c => c.CustomerId == customer.CustomerId);
            if(customerToUpdate == null)
            {
                throw new Exception("Customer not found");
            }
            customerToUpdate = customer;
            db.SaveChanges();
        }
    }
}
