using AutoMapper;
using DREAMHOMES.Controllers.DTOs.ChatHub;
using DREAMHOMES.Models;

namespace DREAMHOMES.Controllers.Mapping_Profiles
{
    public class ChatHubProfile : Profile
    {
        public ChatHubProfile() 
        {
            CreateMap<ChatMessage, ChatMessageDTO>();
        }
    }
}
