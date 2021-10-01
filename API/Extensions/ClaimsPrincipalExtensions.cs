using System.Security.Claims;

namespace API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUsername(this ClaimsPrincipal user)
        {
            // in TokenService class, Claims list
            // JwtRegisteredClaimNames.UniqueName == ClaimType.Name
            return user.FindFirst(ClaimTypes.Name)?.Value; 
        }

        public static int GetUserId(this ClaimsPrincipal user)
        {
            // in TokenService class, Claims list
            // JwtRegisteredClaimNames.NameId == ClaimTypes.NameIdentifier
            return int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value); 
        }
    }
}