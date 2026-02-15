using SyncoraBackend.Enums;

namespace SyncoraBackend.Models.DTOs.Users;

public record UserDTO(int Id, string Email, string Username, string FirstName, string LastName, string Role, DateTime CreationDate, DateTime LastModifiedDate, string? ProfilePictureURL)
{
    public virtual bool Equals(UserDTO? other)
    {
        if (other is null)
        {
            return false;
        }

        return Id == other.Id;
    }
    public override int GetHashCode()
    {
        return Id;
    }
}