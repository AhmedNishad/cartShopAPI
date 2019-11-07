using Shopping.Business.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shopping.Business
{
    public interface IProductService
    {
        ProductBO GetProductById(int productId);
        int QuantityUpdateForProduct(int id, int newQuantity);
        int AddProduct(ProductBO product);
        List<ProductBO> GetProducts();
    }
}
