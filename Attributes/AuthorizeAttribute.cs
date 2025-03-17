using Microsoft.AspNetCore.Authorization;
using TaskManagementWebAPI.Enums;

namespace TaskManagementWebAPI.Attributes;

public class AuthorizeRolesAttribute : AuthorizeAttribute
{
    public AuthorizeRolesAttribute(params UserRole[] roles) : base()
    {
        string[] stringifiedRoles = roles.Select(x => x.ToString()).ToArray();
        Roles = string.Join(",", stringifiedRoles);
    }
}