using AutoMapper;
using SPOTAHOME.Controllers.DTOs.SellerInformation;
using SPOTAHOME.Models;

namespace SPOTAHOME.Controllers.Mappers
{
    public class SellProfile : Profile
    {
        public SellProfile()
        {
            CreateMap<SellerInformationPostPutDTO, SellerInformation>()
                .ForMember(x => x.Documents, y => y.Ignore());

            //CreateMap<SellerInformation, SellerInformationDTO>()
            //    .Include<SellerInformation, SellerInformationDetailedDTO>();

            CreateMap<SellerInformation, SellerInformationLiteDTO>()
                .ForMember(x => x.Country, x => x.MapFrom(y => y.CountryCode));

            CreateMap<SellerInformation, SellerInformationDetailedDTO>()
                .ForMember(x => x.Country, x => x.MapFrom(y => y.CountryCode))
                .ForMember(x => x.Documents, x => x.Ignore());
        }
    }
}
