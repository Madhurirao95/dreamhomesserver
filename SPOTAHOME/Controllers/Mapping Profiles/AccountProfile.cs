using AutoMapper;
using SPOTAHOME.Controllers.DTOs.Account;
using SPOTAHOME.Models;

namespace SPOTAHOME.Controllers.Mappers
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            CreateMap<AccountPostDTO, Account>();
        } 
    }
}
