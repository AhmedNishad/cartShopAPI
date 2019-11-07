using Shopping.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shopping.Data.Interfaces
{
    public interface ICustomerRepository
    {
        int AddCustomer(CustomerDO customer);
        List<CustomerDO> GetCustomers();
        CustomerDO GetCustomerById(int id);

    }
}
