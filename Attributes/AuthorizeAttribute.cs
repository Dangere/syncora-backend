using Microsoft.AspNetCore.Authorization;
using SyncoraBackend.Enums;

namespace SyncoraBackend.Attributes;

public class AuthorizeRolesAttribute : AuthorizeAttribute
{
    public AuthorizeRolesAttribute(params UserRole[] roles) : base()
    {
        string[] stringifiedRoles = roles.Select(x => x.ToString()).ToArray();
        Roles = string.Join(",", stringifiedRoles);
    }
}