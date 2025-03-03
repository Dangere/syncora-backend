using AutoMapper;
using TaskManagementWebAPI.Models.DTOs.Entities;
using TaskManagementWebAPI.Models.DTOs.Tasks;

namespace TaskManagementWebAPI.Middleware;
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<TaskEntity, TaskDTO>();
        CreateMap<TaskDTO, TaskEntity>();


    }
}