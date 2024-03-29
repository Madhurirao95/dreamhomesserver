using AutoMapper;
using DREAMHOMES.Controllers.DTOs.SellerInformation;
using DREAMHOMES.Models;

namespace DREAMHOMES.Controllers.Mappers
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
