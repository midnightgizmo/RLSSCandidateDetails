using Microsoft.Extensions.Options;
using RlssCandidateDetails.JsonWebToken.Models;
using RlssCandidateDetails.Server.Models;
using System.Security.Claims;

namespace RlssCandidateDetails.Server.Middleware
{
    /// <summary>
    /// Checks if the user is logged in by looking for a valid Refresh Token Cookie.
    /// If they are logged in httpContext.User.Identity.IsAuthenticated will be set to true
    /// </summary>
    public class CustomAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        /// <summary>
        /// The settings from the appsettings.json file
        /// </summary>
        private AppSettings _AppSettings;

        public CustomAuthenticationMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings)
        {
            this._next = next;
            this._AppSettings = appSettings.Value;
        }


        public async Task Invoke(HttpContext httpContext)
        {


            // check to see if a JWT has been sent to us in a header.
            // If we have recieved a jwt, check it to make sure it came from us.
            // if it has, and is valid and indate, set the httpContext.User to indicate the user is loggedin.
            // also set any roles the user might have.

            /*
            List<string> headers = new List<string>();
            foreach (var aHeader in httpContext.Request.Headers)
            {
                string key = aHeader.Key;
                string value = aHeader.Value;

                headers.Add(key);
            }*/

            // check if we been sent a jwt
            string? JwtAsString = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            // if we could not find a json web token (one was not sent to us in the header
            if (JwtAsString == null)
            {
                // User probemly is not authenticated anyway, but just to make sure lets create
                // a blank ClaimsPrincipal with a blank claims identity (this will ensure 
                // httpContext.User.Identity.IsAuthenticate is set to false)
                httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
                // Call the next delegate/middleware in the pipeline
                await _next(httpContext);
                return;
            }

            // validate the jwt and get the data stored in it
            //JsonWebToken jsonWebToken = new JsonWebToken(this._AppSettings.jwtsecretKey);
            //JwtUser userInfo = jsonWebToken.getClientDataFromJwt(JwtAsString);

            // if we were unable to valid the jwt that was sent to us
            //if (userInfo == null)
            bool IsValidJWT = JsonWebToken.JwtParser.IsValidJWT(JwtAsString, this._AppSettings.jwtSecretKey);
            JsonWebToken.JwtParser jwt = JsonWebToken.JwtParser.ParseJWT(JwtAsString);
            if (IsValidJWT == false || jwt == null)
            {
                // User probemly is not authenticated anyway, but just to make sure lets create
                // a blank ClaimsPrincipal with a blank claims identity (this will ensure 
                // httpContext.User.Identity.IsAuthenticate is set to false)
                httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
                // Call the next delegate/middleware in the pipeline
                await _next(httpContext);
                return;
            }

            // get all the roles the user has (if any)
            string[] RolesUserHas = jwt.GetPayloadValue("roles").Split(new char[] { ',' },StringSplitOptions.RemoveEmptyEntries);

            // if we did not find any roles assigned to the person, give them a default role of User
            if(RolesUserHas == null || RolesUserHas.Length == 0)
                RolesUserHas = new string[] {"User"};
            
            


            // we cannot directly set httpContext.User.Identity.IsAuthenticate to true (it does not have a set property)
            // To ensure it is set to true we have to create a ClaimsIdentity and set its authentication type.
            // By doing this the httpContext.User.Identity.IsAuthenticate will be set to true
            ClaimsIdentity ci = new ClaimsIdentity("custom");
            // Add the roles this user has for access to the site
            foreach (string aClaim in RolesUserHas)
                ci.AddClaim(new Claim(ClaimTypes.Role, aClaim));
            // create a new ClaimsPrincipal with the Claims Identiy and set it to the user on the context.
            httpContext.User = new ClaimsPrincipal(ci);

            // We don't care if the user is authenticated or not,
            // we are just responsable for saying if they are authenticated, so
            // Call the next delegate/middleware in the pipeline
            await _next(httpContext);

        }


        /// <summary>
        /// If Exists, deletes a cookie by its passed in name
        /// </summary>
        /// <param name="CookieName">Name of cookie to look for and delete</param>
        /// <returns>true if deleted, else false</returns>
        private bool DeleteCookie(string CookieName, HttpContext httpContext)
        {
            bool WasCookieDelete = false;
            if (httpContext.Request.Cookies[CookieName] != null)
            {
                httpContext.Response.Cookies.Delete(CookieName);
                WasCookieDelete = true;
            }

            return WasCookieDelete;
        }


    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class CustomAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomAuthenticationMiddleware>();
        }
    }
}
