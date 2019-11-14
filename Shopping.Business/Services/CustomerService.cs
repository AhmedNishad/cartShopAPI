using AutoMapper;
using Shopping.Business.Entities;
using Shopping.Data.Access;
using Shopping.Data.Entities;
using Shopping.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shopping.Business.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IMapper mapper;
        private readonly ICustomerRepository repository;


        public CustomerService(ICustomerRepository customerRepository, IMapper mapper)
        {
            this.mapper = mapper;
            this.repository = customerRepository;
        }

        public CustomerBO GetCustomerById(int customerId)
        {
            return mapper.Map<CustomerBO>(repository.GetCustomerById(customerId));
        }

        public int AddCustomer(CustomerBO customer)
        {
            var result = repository.AddCustomer(mapper.Map<CustomerDO>(customer));
            if(result == 0)
            {
                throw new Exception("Customer already exists");
            }
            return result;
        }

        public List<CustomerBO> GetCustomers()
        {
            var customers = repository.GetCustomers();
            var mapped = mapper.Map<List<CustomerBO>>(customers);
            return  mapped;
        }
    }
}
