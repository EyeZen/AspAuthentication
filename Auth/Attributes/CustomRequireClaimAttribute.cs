using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AspAuthentication.Auth.Attributes
{
    // --------------------------- Optional ------------------------
    // custom data-annotation attribute can be used on class or method, usage can be extended
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CustomRequireClaimAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _claimName;
        private readonly string _claimValue;

        public CustomRequireClaimAttribute(string claimName, string claimValue)
        {
            _claimName = claimName;
            _claimValue = claimValue;
        }

        // handle authorization
        // TODO: review other `Attribute` lifecycle methods/callbacks
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if(!context.HttpContext.User.HasClaim(_claimName, _claimValue))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
