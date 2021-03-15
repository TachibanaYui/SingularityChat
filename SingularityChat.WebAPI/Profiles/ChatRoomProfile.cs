using AutoMapper;
using SingularityChat.WebAPI.DTOs;
using SingularityChat.WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SingularityChat.WebAPI.Profiles
{

    public class ChatRoomProfile : Profile
    {
        public ChatRoomProfile()
        {
            CreateMap<ChatRoom, RoomReadDto>();

            CreateMap<Message, WSMessageDto>()
                .ForMember(x => x.Message, opt => opt.MapFrom(x => x.Content));

            CreateMap<SystemMessage, WSMessageDto>()
                .ForMember(x => x.Message, opt => opt.MapFrom(x => x.Content));

            CreateMap<ChatMessage, WSChatMessageDto>()
                .ForMember(x => x.Message, opt => opt.MapFrom(x => x.ModifiedMessage));
        }
    }
}
