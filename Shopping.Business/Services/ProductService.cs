using Shopping.Data.Access;
using Shopping.Business.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Shopping.Data.Entities;
using Shopping.Data.Interfaces;

namespace Shopping.Business.Services
{
    public class ProductService : IProductService
    {
        public MapperConfiguration config { get; private set; }

        private readonly IMapper mapper;
        private readonly IProductRepository repository;


        public ProductService(IProductRepository productRepository, IMapper mapper)
        {
            this.mapper = mapper;   
            this.repository = productRepository;
        }

        public int AddProduct(ProductBO product)
        {
            return repository.AddProduct(mapper.Map<ProductDO>(product));
        }

        public List<ProductBO> GetProducts()
        {
            return mapper.Map<List<ProductBO>>(repository.GetProducts());
        }
        public ProductBO GetProductById(int id)
        {
            return mapper.Map<ProductBO>(repository.GetProductById(id));
        }

        public int QuantityUpdateForProduct(int id, int newQuantity)
        {
            return repository.QuantityUpdateForProduct(id, newQuantity);
        }

    }
}
