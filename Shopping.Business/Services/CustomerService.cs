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
            //var config = new MapperConfiguration(cfg => {
            //    cfg.CreateMap<CustomerBO, Customer>().ReverseMap();
            //    cfg.CreateMap<List<CustomerBO>, List<Customer>>().ReverseMap();
            //});
            this.mapper = mapper;
            this.repository = customerRepository;
        }

        public CustomerBO GetCustomerById(int customerId)
        {
            return mapper.Map<CustomerBO>(repository.GetCustomerById(customerId));
        }

        public int AddCustomer(CustomerBO customer)
        {
           return  repository.AddCustomer(mapper.Map<CustomerDO>(customer));
        }

        public List<CustomerBO> GetCustomers()
        {
            var customers = repository.GetCustomers();
            var mapped = mapper.Map<List<CustomerBO>>(customers);
            return  mapped;
        }
    }
}
