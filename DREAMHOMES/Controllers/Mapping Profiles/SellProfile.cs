using AutoMapper;
using DREAMHOMES.Controllers.DTOs.SellerInformation;
using DREAMHOMES.Models;
using NetTopologySuite.Geometries;

namespace DREAMHOMES.Controllers.Mappers
{
    public class SellProfile : Profile
    {
        public SellProfile()
        {
            CreateMap<SellerInformationPostPutDTO, SellerInformation>()
                .ForMember(x => x.Location, y => y.MapFrom(x => new Point(x.CoordinateX, x.CoordinateY)))
                .ForMember(x => x.Documents, y => y.Ignore());

            CreateMap<SellerInformation, SellerInformationLiteGetDTO>()
                .ForMember(x => x.Country, x => x.MapFrom(y => y.CountryCode));

            CreateMap<SellerInformation, SellerInformationDetailedDTO>()
                .ForMember(x => x.Country, x => x.MapFrom(y => y.CountryCode))
                .ForMember(x => x.CoordinateX, x => x.MapFrom(y => y.Location.Coordinate.X))
                .ForMember(x => x.CoordinateY, x => x.MapFrom(y => y.Location.Coordinate.Y))
                .ForMember(x => x.DocumentList, x => x.Ignore());

            CreateMap<SellerInformation, SellerInformationDetailedGetDTO>()
                .ForMember(x => x.Country, x => x.MapFrom(y => y.CountryCode))
                .ForMember(x => x.CoordinateX, x => x.MapFrom(y => y.Location.Coordinate.X))
                .ForMember(x => x.CoordinateY, x => x.MapFrom(y => y.Location.Coordinate.Y))
                .ForMember(x => x.DocumentList, x => x.Ignore());
        }
    }
}
