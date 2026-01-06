using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using SyncoraBackend.Middleware;
using SyncoraBackend.Models.DTOs.Groups;
using SyncoraBackend.Models.DTOs.Tasks;
using SyncoraBackend.Models.DTOs.Users;
using SyncoraBackend.Models.Entities;

namespace SyncoraBackend.Models.DTOs.Sync;

public record SyncPayload : IValidatableObject
{

    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

    public List<UserDTO> Users { get; private set; } = [];

    public List<GroupDTO> Groups { get; private set; } = [];

    public List<TaskDTO> Tasks { get; private set; } = [];

    public List<int> KickedGroupsIds { get; private set; } = [];

    public List<int> DeletedGroupsIds { get; private set; } = [];

    public List<int> DeletedTasksIds { get; private set; } = [];

    // public SyncPayload(List<UserEntity>? Users = null, List<GroupEntity>? Groups = null, List<TaskEntity>? Tasks = null, List<int>? KickedGroupsIds = null, List<int>? DeletedGroupsIds = null, List<int>? DeletedTasksIds = null)
    // {
    //     Mapper mapper = new(new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile())));

    //     Timestamp = DateTime.UtcNow;
    //     this.Users = Users?.Select(mapper.Map<UserDTO>).ToList() ?? [];
    //     this.Groups = Groups?.Select(mapper.Map<GroupDTO>).ToList() ?? [];
    //     this.Tasks = Tasks?.Select(mapper.Map<TaskDTO>).ToList() ?? [];
    //     this.KickedGroupsIds = KickedGroupsIds ?? [];
    //     this.DeletedGroupsIds = DeletedGroupsIds ?? [];
    //     this.DeletedTasksIds = DeletedTasksIds ?? [];

    // }


    public static SyncPayload FromDto(List<UserDTO>? Users = null, List<GroupDTO>? Groups = null, List<TaskDTO>? Tasks = null, List<int>? KickedGroupsIds = null, List<int>? DeletedGroupsIds = null, List<int>? DeletedTasksIds = null)
    {
        SyncPayload payload = new()
        {
            Users = Users ?? [],
            Groups = Groups ?? [],
            Tasks = Tasks ?? [],
            KickedGroupsIds = KickedGroupsIds ?? [],
            DeletedGroupsIds = DeletedGroupsIds ?? [],
            DeletedTasksIds = DeletedTasksIds ?? []
        };

        return payload;

    }

    public static SyncPayload FromEntity(List<UserEntity>? Users = null, List<GroupEntity>? Groups = null, List<TaskEntity>? Tasks = null, List<int>? KickedGroupsIds = null, List<int>? DeletedGroupsIds = null, List<int>? DeletedTasksIds = null)
    {
        Mapper mapper = new(new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile())));
        SyncPayload payload = new()
        {
            Users = Users?.Select(mapper.Map<UserDTO>).ToList() ?? [],
            Groups = Groups?.Select(mapper.Map<GroupDTO>).ToList() ?? [],
            Tasks = Tasks?.Select(mapper.Map<TaskDTO>).ToList() ?? [],
            KickedGroupsIds = KickedGroupsIds ?? [],
            DeletedGroupsIds = DeletedGroupsIds ?? [],
            DeletedTasksIds = DeletedTasksIds ?? []
        };

        return payload;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Users == null && Groups == null && Tasks == null && KickedGroupsIds == null && DeletedGroupsIds == null && DeletedTasksIds == null)
            yield return new ValidationResult("At least one field must contain data",
                [nameof(Users), nameof(Groups), nameof(Tasks), nameof(KickedGroupsIds), nameof(DeletedGroupsIds), nameof(DeletedTasksIds)]);
    }
}
