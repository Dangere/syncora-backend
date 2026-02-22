using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SyncoraBackend.Data;
using SyncoraBackend.Models.DTOs.Sync;
using SyncoraBackend.Models.DTOs.Users;
using SyncoraBackend.Models.Entities;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Services;

public class UsersService(ImagesService imagesService, ClientSyncService clientSyncService, SyncoraDbContext dbContext, IMapper mapper)
{
    private readonly ImagesService _imagesService = imagesService;
    private readonly ClientSyncService _clientSyncService = clientSyncService;
    private readonly SyncoraDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;



    public async Task<Result<string>> UpdateUserProfilePicture(int userId, string imageUrl)
    {

        // TODO: Validate image URL
        Result<bool> result = await _imagesService.ValidateUrlString(imageUrl);
        if (!result.IsSuccess)
            return Result<string>.Error(result.ErrorMessage!, result.ErrorStatusCode);

        // Update user
        UserEntity? user = await _dbContext.Users.Include(u => u.OwnedGroups).Include(u => u.GroupMemberships).FirstAsync(u => u.Id == userId);
        if (user == null)
            return Result<string>.Error("User does not exist.", StatusCodes.Status404NotFound);
        user.ProfilePictureURL = imageUrl;
        user.LastModifiedDate = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        HashSet<int> userGroupIds = user.OwnedGroups.Select(g => g.Id).Union(user.GroupMemberships.Select(m => m.GroupId)).ToHashSet();


        // TODO: Notify users
        await _clientSyncService.PushPayloadToGroups(userGroupIds, SyncPayload.FromEntity(Users: [user]));


        return Result<string>.Success("Profile picture updated.");
    }

    public async Task<Result<string>> UpdateUserProfile(UpdateUserProfileDTO updateUserProfileDTO, int userId)
    {
        UserEntity? user = await _dbContext.Users.Include(u => u.OwnedGroups).Include(u => u.GroupMemberships).FirstAsync(u => u.Id == userId);
        if (user == null)
            return Result<string>.Error("User does not exist.", StatusCodes.Status404NotFound);

        if (updateUserProfileDTO.FirstName != null)
            if (!Validators.ValidateName(updateUserProfileDTO.FirstName))
            {
                return Result<string>.Error("First name is not in valid format.");
            }

        if (updateUserProfileDTO.LastName != null)
            if (!Validators.ValidateName(updateUserProfileDTO.LastName))
            {
                return Result<string>.Error("Last name is not in valid format.");
            }

        user.FirstName = updateUserProfileDTO.FirstName ?? user.FirstName;
        user.LastName = updateUserProfileDTO.LastName ?? user.LastName;
        user.Preferences = updateUserProfileDTO.Preferences ?? user.Preferences;

        if (updateUserProfileDTO.Username != null)
        {

            if (!Validators.ValidateUsername(updateUserProfileDTO.Username))
            {
                return Result<string>.Error("Username is not in valid format.");
            }

            UserEntity? userWithSameEmailOrUserName = await _dbContext.Users.FirstOrDefaultAsync(u => EF.Functions.ILike(u.Username, updateUserProfileDTO.Username));
            if (userWithSameEmailOrUserName != null)
            {
                return Result<string>.Error("Username is already in use.", StatusCodes.Status409Conflict);
            }
        }
        user.LastModifiedDate = DateTime.UtcNow;


        HashSet<int> userGroupIds = user.OwnedGroups.Select(g => g.Id).Union(user.GroupMemberships.Select(m => m.GroupId)).ToHashSet();


        // TODO: Notify users
        await _clientSyncService.PushPayloadToGroups(userGroupIds, SyncPayload.FromEntity(Users: [user]));
        return Result<string>.Success("Profile updated.");

    }
}