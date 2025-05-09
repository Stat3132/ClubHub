using AutoMapper;
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<UserDTO, User>();        
        //CreateMap<OrderDTO, Order>();
        //CreateMap<FoodDTO, Food>();
        //CreateMap<List<FoodDTO>, List<Food>>();
    }
}