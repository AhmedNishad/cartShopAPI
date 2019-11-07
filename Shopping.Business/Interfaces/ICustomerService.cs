using Shopping.Business.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shopping.Business
{
    public interface ICustomerService
    {
        CustomerBO GetCustomerById(int customerId);
        int AddCustomer(CustomerBO customer);
        List<CustomerBO> GetCustomers();
    }
}
