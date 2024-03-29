using AutoMapper;
using SPOTAHOME.Models;

namespace SPOTAHOME.Services.MappingProfile
{
    public class SellServiceMappingProfile : Profile
    {
        public SellServiceMappingProfile()
        {
            CreateMap<SellerInformation, SellerInformation>()
                .ForMember(x => x.Id, y => y.Ignore())
                .ForMember(x => x.Documents, y => y.Ignore())
                .ForMember(x => x.User, y => y.Ignore())
                .ForMember(x => x.UserId, y => y.Ignore());
        }

    }
}
