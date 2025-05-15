using AutoMapper;
namespace UserService.Models
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UserDTO, User>()
                .ForMember(dest => dest.password, opt => opt.Ignore()) // ignore password
                .ForMember(dest => dest.userID, opt => opt.Ignore()); // Ignore userID
        }
    }
}