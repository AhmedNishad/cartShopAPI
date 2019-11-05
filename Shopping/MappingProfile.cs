using Shopping.Business.Entities;
using Shopping.Data.Entities;
using Shopping.Entities;
using System.Collections.Generic;

namespace Shopping
{
    internal class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile()
        {
                CreateMap<ProductDO, ProductBO>().ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ProductId)).ReverseMap();
                CreateMap<CustomerDO, CustomerBO>().ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CustomerId)).ReverseMap();
                //CreateMap<List<CustomerDO>, List<CustomerBO>>().ReverseMap();
                CreateMap<OrderDO, OrderBO>().ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OrderId)).ReverseMap();
                CreateMap<OrderLineItemDO, OrderLineItemBO>().ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.LineId)).ReverseMap();
                CreateMap<Product, ProductBO>().ReverseMap();
                CreateMap<Customer, CustomerBO>().ReverseMap();
                //CreateMap<List<Customer>, List<CustomerBO>>().ReverseMap();
                CreateMap<Order, OrderBO>().ReverseMap();
                CreateMap<OrderLineItem, OrderLineItemBO>().ReverseMap();

        }
    }
}