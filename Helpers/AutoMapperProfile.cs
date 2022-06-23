using AutoMapper;
using Megastonks.Entities;
using Megastonks.Models.Account;

namespace Megastonks.Helpers
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap<Account, AuthenticateResponse>();
		}
	}
}

