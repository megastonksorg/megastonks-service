using AutoMapper;
using Megastonks.Entities;
using Megastonks.Entities.Message;
using Megastonks.Models.Account;
using Megastonks.Models.Message;

namespace Megastonks.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Account, AuthenticateResponse>();
            CreateMap<RegisterRequest, Account>();
            CreateMap<Account, RegisterResponse>();
            CreateMap<Message, MessageResponse>()
                .ReverseMap();
            CreateMap<MessageKeyModel, MessageKey>()
                .ReverseMap();
            CreateMap<MessageReaction, MessageResponse.Reaction>()
              .ForMember(d => d.SenderWalletAddress, opt => opt.MapFrom(src => src.Sender.WalletAddress))
              .ReverseMap()
              .ForPath(s => s.Sender.WalletAddress, opt => opt.MapFrom(src => src.SenderWalletAddress));
        }
    }
}