using Shopping.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shopping.Data.Interfaces
{
    public interface IProductRepository
    {
        int AddProduct(ProductDO product);
        List<ProductDO> GetProducts();
        ProductDO GetProductById(int id);
        int QuantityUpdateForProduct(int ProductId, int newQuantity);
        void DeleteProductById(int id);
        int UpdateProduct(int id, ProductDO productDO);
    }
}
