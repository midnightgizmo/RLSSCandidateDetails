using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using RlssCandidateDetails.Client.Classes.Server;
using RlssCandidateDetails.Client.Models;
using RlssCandidateDetails.Client.Models.Server.ResponseData;
using RlssCandidateDetails.JsonWebToken;
using System.Security.Claims;

namespace RlssCandidateDetails.Client.Classes.Authentication
{
    /// <summary>
    /// Requires the package Microsoft.AspNetCore.Components.Authorization
    /// </summary>
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        // create a ClaimsPrincipal with a blank ClaimsIdentity (this will make authentication fail)
        private ClaimsPrincipal _claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        // Used to get cookies from the browser
        private IJSRuntime _jSRuntime;
        public CustomAuthStateProvider(IJSRuntime JSRuntime, IConfiguration configuration, AppSettings appSettings, HttpClient httpClient)
        {
            this._jSRuntime = JSRuntime;

            // convert the appsettings.json file to strongly typed class
            //this.AppSettings = configuration.GetSection("AppSettings").Get<AppSettings>();
            this.AppSettings = appSettings;

            this.HttpClient = httpClient;


        }
        public HttpClient HttpClient { get; set; }
        public AppSettings AppSettings { get; set; }
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {

            // Info on how to add roles to authentication
            //
            // ://code-maze.com/blazor-webassembly-role-based-authorization/

            ClaimsIdentity? claimsIdentity;

            // get the jwt value that is stored ont he AppSettings
            string jsonWebTokenAsString = AppSettings.JsonWebToken;

            // if there is no Json web token
            if (jsonWebTokenAsString == null || jsonWebTokenAsString == String.Empty)
            {
                // try and get a new access token from the server
                jsonWebTokenAsString = await this.CallRefreshToken();
                // if we were unable to get a new access token
                if (jsonWebTokenAsString == null || jsonWebTokenAsString == string.Empty)
                {
                    // create a blank claims identity to indicate the user is not authenticated
                    claimsIdentity = new ClaimsIdentity();
                    this._claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    // return an AuthenitcationState
                    return new AuthenticationState(this._claimsPrincipal);

                }
                else// update the app settings with the new json web token
                    AppSettings.JsonWebToken = jsonWebTokenAsString;
            }

            // used to convert the jwt string to redable text
            JwtParser? jsonWebToken = JwtParser.ParseJWT(jsonWebTokenAsString);


            // convert the jwt and get the payload data from it
            //List<Claim> clamesList = new List<Claim>(jsonWebToken.GetPayload(jsonWebTokenAsString));


            // if we could not find the expiration date or the expiration date is in the past
            if (jsonWebToken == null || jsonWebToken.IsJsonWebTokenInDate() == false)
            {
                // try and get a new access token from the server
                jsonWebTokenAsString = await this.CallRefreshToken();
                if (jsonWebTokenAsString == string.Empty)
                {
                    // create a blank claims identity to indicate the user is not authenticated
                    claimsIdentity = new ClaimsIdentity();
                    this._claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    // return an AuthenitcationState
                    return new AuthenticationState(this._claimsPrincipal);
                }
                // we have recieved a new access token
                else
                {
                    // update the app settings witht he new json web token
                    AppSettings.JsonWebToken = jsonWebTokenAsString;
                    //https://jasonwatmore.com/post/2022/01/24/net-6-jwt-authentication-with-refresh-tokens-tutorial-with-example-api#:~:text=The%20JWT%20is%20used%20for,or%20just%20before)%20they%20expire.
                    // parse the access token
                    jsonWebToken = JwtParser.ParseJWT(jsonWebTokenAsString);
                    
                    // if we could not find the expiration date or the expiration date is in the past in the new access token
                    if (jsonWebToken == null || jsonWebToken.IsJsonWebTokenInDate() == false)
                    {
                        // create a blank claims identity to indicate the user is not authenticated
                        claimsIdentity = new ClaimsIdentity();
                        this._claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        // return an AuthenitcationState
                        return new AuthenticationState(this._claimsPrincipal);
                    }
                }
            }

            // if we get this far we have a json web token and it is in date

            // get the name and roles the user has int he jwt and add them to the claims identity.
            claimsIdentity = GetClaimsIdentity(jsonWebToken);
            // if one or more of the claims was not found, claimsIdentity will be null
            if (claimsIdentity == null)
                // create a blank claims Identity. This will mean the user is NOT authenticated
                claimsIdentity = new ClaimsIdentity();
            




            // add the claims identity to the Claims principal
            this._claimsPrincipal = new ClaimsPrincipal(claimsIdentity);


            // return an AuthenitcationState
            return new AuthenticationState(this._claimsPrincipal);

        }



        /// <summary>
        /// Call this when the Cookie has been added to the clients web browser.
        /// It will update the Authentication state to show the user is logged in.
        /// </summary>
        public void Login()
        {
            this.NotifyAuthenticationStateChanged(this.GetAuthenticationStateAsync());
        }
        public void LogoutUser()
        {
            // remove the json web token.
            AppSettings.JsonWebToken = String.Empty;
            // remove the cookie (we can't do this though so we need to ask the server to do it)


            this.NotifyAuthenticationStateChanged(this.GetAuthenticationStateAsync());
        }

        public void ForceAuthenticationStateChangeEvent()
        {
            this.NotifyAuthenticationStateChanged(this.GetAuthenticationStateAsync());
        }


        private async void DeleteCookie(string CookieName)
        {
            await this._jSRuntime.InvokeVoidAsync("expireCookie", CookieName);

        }

        /// <summary>
        /// Gets a list of all cookies in the format "key=value"
        /// </summary>
        /// <returns></returns>
        private async Task<string[]> getCookies()
        {
            string cookies;
            string[] cookiesArray;
            cookies = await this._jSRuntime.InvokeAsync<string>("getCookies", "test");
            Console.WriteLine(cookies);
            cookiesArray = cookies.Split(new char[] { ';' });
            return cookiesArray;
        }


        /// <summary>
        /// Looks for the requested cookie and returns its value. returns string.empty if not found
        /// </summary>
        /// <param name="cookieName">The name of the cookie to look for</param>
        /// <param name="cookies">The list of cookies to seach through</param>
        /// <returns></returns>
        private string getCookieValue(string cookieName, string[] cookies)
        {
            string CookieValue = "";

            foreach (string cookie in cookies)
            {
                int seperatorPosition = cookie.IndexOf('=');
                if (seperatorPosition == -1)
                    continue;

                string name = cookie.Substring(0, seperatorPosition);

                if (name == cookieName)
                {
                    CookieValue = cookie.Substring(seperatorPosition + 1);
                    break;
                }
            }

            return CookieValue;
        }

        /// <summary>
        /// Returns a claims identity with the users name and there roles populated
        /// </summary>
        /// <param name="JWT"></param>
        /// <returns>populated ClaimsIdentity or null of one or more claims could not be found</returns>
        private ClaimsIdentity? GetClaimsIdentity(JwtParser JWT)
        {
            // will hold a list of all the claims we want to add to the ClaimsIdentity
            List<Claim> claims = new List<Claim>();
            
            // try and get the users name from the jwt
            string usersName = JWT.GetPayloadValue("name");
            // if we could not find the users name return null to indicate not all data was found in the jwt
            if (usersName == string.Empty)
                return null;

            // try and get the users roles they have (roles grant access to differnet parts of a site e.g. admin accesss)
            string userRoles = JWT.GetPayloadValue("roles");
            if (userRoles == string.Empty)
                return null;

            // add the users name
            claims.Add(new Claim(ClaimTypes.Name, usersName));
            // roles are in one string split by a comma. go through each one add add them as a role claim
            foreach (string role in userRoles.Split(",", StringSplitOptions.RemoveEmptyEntries))
                claims.Add(new Claim(ClaimTypes.Role, role));

            // add the name and roles to the ClaimsIdentity
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "user");


            return claimsIdentity;
        }

        /// <summary>
        /// Call this method when we don't have a authentication token (json web token).
        /// It will attempt to get a new json web token by reuqesting one from the server.
        /// </summary>
        /// <returns>json web token or string.Empty</returns>
        private async Task<string> CallRefreshToken()
        {
            // call the servers "/Authentication/RefreshToken" path to see if we are able
            // to get a new Authentication json web token.
            string jsonWebToken = string.Empty;

            AuthenticationCommunication serverCommunication = new AuthenticationCommunication(this.HttpClient, this.AppSettings);
            SereverResponsePlainText serverResponse;
            // make the call to the server
            serverResponse = await serverCommunication.RefreshToken();
            // if we got a good response back (Server has sent us an access token)
            if (serverResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                jsonWebToken = serverResponse.ReturnValue;
            }

            // returns the new json web token or an empty string if it was not returned.
            return jsonWebToken;

        }
    }
}
