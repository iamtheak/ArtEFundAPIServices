using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Security.Claims;

namespace ArtEFundAPIServices.Attributes
{
    public class RoleCheckAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _role;

        public RoleCheckAttribute(string role)
        {
            _role = role;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var roleClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role && c.Value == _role);
            if (roleClaim == null)
            {
                context.Result = new JsonResult(new { message = "You do not have the required role." })
                {
                    StatusCode = 403 // Forbidden
                };
            }
        }
    }
}