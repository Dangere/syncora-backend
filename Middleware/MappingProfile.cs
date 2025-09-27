using AutoMapper;
using SyncoraBackend.Models.Entities;
using SyncoraBackend.Models.DTOs.Tasks;
using SyncoraBackend.Models.DTOs.Users;
using SyncoraBackend.Models.DTOs.Groups;
using SyncoraBackend.Enums;

namespace SyncoraBackend.Middleware;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<TaskEntity, TaskDTO>().ForMember(dest => dest.AssignedTo, opt => opt.MapFrom(src => src.AssignedTo.Select(m => m.Id).ToArray()));
        CreateMap<UserEntity, int>().ConvertUsing(u => u.Id);

        CreateMap<GroupEntity, GroupDTO>().ForMember(dest => dest.GroupMembers, opt => opt.MapFrom(src => src.GroupMembers.Select(m => m.UserId)));
        CreateMap<GroupMemberEntity, int>().ConvertUsing(m => m.UserId);



        // Configure the mapping from UserEntity to UserDTO and vice versa
        // While converting the role enum to string and vice versa
        CreateMap<UserEntity, UserDTO>()
        .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));
        CreateMap<UserDTO, UserEntity>()
        .ForMember(dest => dest.Role, opt => opt.MapFrom(src => Enum.Parse<UserRole>(src.Role)));



    }
}