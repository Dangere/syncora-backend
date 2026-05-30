using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SyncoraBackend.Data;
using SyncoraBackend.Models;
using SyncoraBackend.Models.DTOs.Sync;
using SyncoraBackend.Models.DTOs.Users;
using SyncoraBackend.Models.Entities;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Services;

public class UsersService(ImagesService imagesService, ClientSyncService clientSyncService, SyncoraDbContext dbContext, ILogger<UsersService> logger, IMapper mapper, UserRequestContext userRequestContext)
{
    private readonly ImagesService _imagesService = imagesService;
    private readonly ClientSyncService _clientSyncService = clientSyncService;
    private readonly SyncoraDbContext _dbContext = dbContext;
    private readonly ILogger<UsersService> _logger = logger;
    private readonly IMapper _mapper = mapper;

    private readonly UserRequestContext _userRequestContext = userRequestContext;



    public async Task<Result<UserDTO>> GetUser(string username)
    {
        UserEntity? user = await _dbContext.Users.FirstOrDefaultAsync(u => EF.Functions.ILike(u.Username, username));

        if (user == null)
            return Result<UserDTO>.Error("User does not exist.", ErrorCodes.USER_NOT_FOUND, StatusCodes.Status404NotFound);

        return new Result<UserDTO>(_mapper.Map<UserDTO>(user));
    }
    /// <summary>
    ///     Updates the user profile picture
    ///     Returns an error if the image URL is invalid 
    ///     Pushes a sync payload to all connected clients
    /// </summary>
    /// <param name="imageUrl"></param>
    /// <returns></returns>
    public async Task<Result<string>> UpdateUserProfilePicture(string imageUrl)
    {

        // TODO: Validate image URL
        Result<bool> result = await _imagesService.ValidateUrlString(imageUrl);
        if (!result.IsSuccess)
            return Result<string>.ErrorFrom(result);

        // Update user
        UserEntity? user = await _dbContext.Users.Include(u => u.OwnedGroups).Include(u => u.GroupMemberships).FirstAsync(u => u.Id == _userRequestContext.UserId);
        if (user == null)
            return Result<string>.Error("User does not exist.", ErrorCodes.USER_NOT_FOUND, StatusCodes.Status404NotFound);
        user.ProfilePictureURL = imageUrl;
        user.LastModifiedDate = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();


        //  Notify users
        await _clientSyncService.PushPayloadToPeople(await GetRelatedUserIds(_userRequestContext.UserId), SyncPayload.FromEntity(Users: [user]));


        return Result<string>.Success("Profile picture updated.");
    }
    /// <summary>
    ///     Updates the user profile
    ///     Returns an error if the user details are the same or if the username is already in use
    ///     Pushes a sync payload to all connected clients
    /// </summary>
    /// <param name="updateUserProfileDTO"></param>
    /// <returns></returns>
    public async Task<Result<string>> UpdateUserProfile(UpdateUserProfileDTO updateUserProfileDTO)
    {
        UserEntity? user = await _dbContext.Users.Include(u => u.OwnedGroups).Include(u => u.GroupMemberships).FirstAsync(u => u.Id == _userRequestContext.UserId);
        if (user == null)
            return Result<string>.Error("User does not exist.", ErrorCodes.USER_NOT_FOUND, StatusCodes.Status404NotFound);

        user.FirstName = updateUserProfileDTO.FirstName ?? user.FirstName;
        user.LastName = updateUserProfileDTO.LastName ?? user.LastName;

        if (updateUserProfileDTO.Preferences != null)
            user.Preferences = user.Preferences.UpdateFromDTO(updateUserProfileDTO.Preferences);

        if (updateUserProfileDTO.Username != null)
        {

            UserEntity? userWithSameEmailOrUserName = await _dbContext.Users.FirstOrDefaultAsync(u => EF.Functions.ILike(u.Username, updateUserProfileDTO.Username));
            if (userWithSameEmailOrUserName != null)
            {
                return Result<string>.Error("Username is already in use.", ErrorCodes.USERNAME_ALREADY_IN_USE, StatusCodes.Status409Conflict);
            }

            user.Username = updateUserProfileDTO.Username;
        }
        user.LastModifiedDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();


        // Notify users
        if (!updateUserProfileDTO.IsUpdatingPreferencesOnly)
            await _clientSyncService.PushPayloadToPeople(await GetRelatedUserIds(_userRequestContext.UserId), SyncPayload.FromEntity(Users: [user]));
        return Result<string>.Success("Profile updated.");

    }

    /// <summary>
    ///     Returns a list of user ids related (in a group with the user) to the user
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<int>> GetRelatedUserIds(int userId)
    {
        // Returns users that share a group with a user 
        // Such as, users who own a group and the user is a member
        // And, users who are a member of a group and the user is a member
        // And, users who are a member of a group and the user is the owner

        var rawRelatedUserIds = await _dbContext.Groups.Include(g => g.GroupMembers)
            .Where(g => (g.OwnerUserId == userId ||
                        g.GroupMembers.Any(m => m.UserId == userId && m.KickedAt == null)) && g.DeletedAt == null)
            .Select(g => new { g.OwnerUserId, g.GroupMembers })
            .ToListAsync();

        var relatedUserIds = new HashSet<int>(rawRelatedUserIds.SelectMany(g => new int[] { g.OwnerUserId }.Union(g.GroupMembers.Where(m => m.KickedAt == null).Select(m => m.UserId))));

        relatedUserIds.Remove(userId);


        _logger.LogInformation("Related user ids: {RelatedUserIds}", relatedUserIds);
        return [.. relatedUserIds];
    }
}