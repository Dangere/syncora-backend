using AutoMapper;
using TaskManagementWebAPI.Models.Entities;
using TaskManagementWebAPI.Models.DTOs.Tasks;
using TaskManagementWebAPI.Models.DTOs.Users;

namespace TaskManagementWebAPI.Middleware;
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<TaskEntity, TaskDTO>();
        CreateMap<TaskDTO, TaskEntity>();

        CreateMap<UserEntity, UserDTO>();
        CreateMap<UserDTO, UserEntity>();



    }
}