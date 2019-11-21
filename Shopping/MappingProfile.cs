using Shopping.Business.Entities;
using Shopping.Data.Entities;
using Shopping.ViewModels;
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
                CreateMap<ProductVM, ProductBO>().ReverseMap();
                CreateMap<CustomerVM, CustomerBO>().ReverseMap();
                //CreateMap<List<Customer>, List<CustomerBO>>().ReverseMap();
                CreateMap<OrderVM, OrderBO>().ReverseMap();
                CreateMap<OrderLineItemVM, OrderLineItemBO>().ReverseMap();

        }
    }
}