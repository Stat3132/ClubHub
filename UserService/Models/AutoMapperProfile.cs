using AutoMapper;
namespace UserService.Models
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UserDTO, User>();
        }
    }
}