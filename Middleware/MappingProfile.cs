using AutoMapper;
using TaskManagementWebAPI.Models.Entities;
using TaskManagementWebAPI.Models.DTOs.Tasks;
using TaskManagementWebAPI.Models.DTOs.Users;
using TaskManagementWebAPI.Enums;

namespace TaskManagementWebAPI.Middleware;
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<TaskEntity, TaskDTO>();
        CreateMap<TaskDTO, TaskEntity>();


        // Configure the mapping from UserEntity to UserDTO and vice versa
        // While converting the role enum to string and vice versa
        CreateMap<UserEntity, UserDTO>()
        .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));
        CreateMap<UserDTO, UserEntity>()
        .ForMember(dest => dest.Role, opt => opt.MapFrom(src => Enum.Parse<UserRole>(src.Role)));



    }
}