using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace RlssCandidateDetails.Server.Attributes
{
    /// <summary>
    /// This gets called when a controller or method has the [Authorize] attribute added to it.
    /// Will check if the user is authorized to access the contoller/meothod the attribute is attached too.
    /// Sends back to the client a 403 response if they are not authorized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        /// <summary>
        /// The type of Authorization the attribute is looking for (defaults to NormalUser)
        /// </summary>
        public AuthorizeType AuthorizationType = AuthorizeType.User;
        /// <summary>
        /// Check if user is Authorized by looking for the "user" in the HttpContent.Items
        /// </summary>
        /// <param name="context">Used to check if User exists in the HttpContext.Items</param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {



            // Dynamicly checks to see if user has access to the current controllers method
            // this means we can add to the Enum value more roles and not worry about having to change this code.

            // get all the roles the user has as enum values
            // These will have been set in the AuthenticationController middleware (it gets them from the jwt)
            var rolesUserHas = context.HttpContext.User.Claims.Where(s => s.Type == System.Security.Claims.ClaimTypes.Role)
                                                          .Select(c => c.Value);

            // after the foreach loop we will check this value and respond ocurdingly
            bool DoesUserHaveAccessToMethod = false;
            // go through each role the user has
            foreach (string aRole in rolesUserHas)
            {
                AuthorizeType ParsedValue;
                // convert the users role to an enum value of type AuthorizeType
                if (Enum.TryParse<AuthorizeType>(aRole, out ParsedValue) == true)
                {

                    // does this role we looking at (the users role) exist in the controller method
                    // we are looking at.
                    if (this.AuthorizationType.HasFlag(ParsedValue))
                    {
                        // user has access to the controllers method we are trying to go to
                        DoesUserHaveAccessToMethod = true;
                        // no need to do any more checks because we know we have access
                        break;
                    }
                }
            }

            // if we don't have access to the controllers method respond with a 403
            if (DoesUserHaveAccessToMethod == false)
            {
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status403Forbidden };
                return;
            }

            // by getting this far we are saying the user has access to the controllers method
            return;


        }
    }

    /// <summary>
    /// The types of user access that are avalable
    /// </summary>
    [Flags]
    public enum AuthorizeType
    {
        Admin = 1, // next number is double this number (2)
        User = 2, // next number is double this number (4)
    }
}
