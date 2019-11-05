using AutoMapper;
using Shopping.Business.Entities;
using Shopping.Data.Entities;

namespace Shopping.Business
{
    internal class AppProfile : Profile
    {
        public AppProfile()
        {
            CreateMap<ProductDO, ProductBO>();

        }
    }
}