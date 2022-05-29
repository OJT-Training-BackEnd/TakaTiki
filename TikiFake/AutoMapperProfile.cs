using AutoMapper;
using TikiFake.Dtos.User;
using TikiFake.Models;

namespace TikiFake
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserRegisterDto>();
            CreateMap<UserRegisterDto, User>();
        }
    }
}
