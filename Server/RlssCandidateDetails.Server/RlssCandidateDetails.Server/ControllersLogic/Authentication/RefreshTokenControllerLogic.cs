using RlssCandidateDetails.JsonWebToken;
using RlssCandidateDetails.RefreshToken;
using RlssCandidateDetails.RefreshToken.Models;
using RlssCandidateDetails.Server.Database.dbTables;
using RlssCandidateDetails.Server.Database;
using RlssCandidateDetails.Server.Models;
using RlssCandidateDetails.Server.Models.Admin;
using RlssCandidateDetails.Server.Models.Candidate;

namespace RlssCandidateDetails.Server.ControllersLogic.Authentication
{
    public class RefreshTokenControllerLogic
    {
        public ControllerLogicReturnValue Process(AppSettings appSettings, HttpRequest httpRequest, HttpResponse httpResponse)
        {
            ControllerLogicReturnValue ReturnValue;

            // try and get the the encrypted refreshToken which should be stored in a cookie
            ReturnValue = this.GetRefreshTokenCookie(httpRequest, appSettings);
            // if we were unable to get the encrypted refresh token
            if (ReturnValue.HasErrors)
                return ReturnValue;


            // try and dycrpt the Refresh Token we were sent from the client
            ReturnValue = this.DycryptRefreshToken((string)ReturnValue.ReturnValue, appSettings,httpResponse);
            // If we were unable to dycrypt the refresh token we were sent from the client
            if (ReturnValue.HasErrors)
                return ReturnValue;

            // update the Refresh Token (updates its version number and increases its expiration date).
            // Also creates the Cookie on the httpResponse which gets sent back to the user
            ReturnValue = this.UpdateTokenToNewVersion((Token)ReturnValue.ReturnValue, appSettings, httpResponse);

            if (ReturnValue.HasErrors)
                return ReturnValue;

            // get the updated token and its encrypted string value
            //Tuple< Token, string> TokenData = (Tuple<Token, string>)ReturnValue.ReturnValue;
            (Token, string) TokenData = (((Token, string))ReturnValue.ReturnValue);
            // try and create the json web token
            ReturnValue = this.CreateJsonWebToken(appSettings, TokenData.Item1.UserID, httpResponse);
            if (ReturnValue.HasErrors)
            {
                // delete the Refresh Token Cookie
                this.DeleteRefreshTokenCookie(appSettings,httpResponse);
                return ReturnValue;
            }

            // We should now have an updated refresh token and a new Json web token
            // ReturnValue.ReturnValue should be set to the new Json web token
            return ReturnValue;
        }




        /// <summary>
        /// Get the Encrypted Refresh Token which should be stored in a cookie we were sent from the client
        /// </summary>
        /// <param name="httpRequest">Holds the Cookie which has the encrypted RefreshToken</param>
        /// <param name="appSettings"></param>
        /// <returns></returns>
        private ControllerLogicReturnValue GetRefreshTokenCookie(HttpRequest httpRequest, AppSettings appSettings)
        {
            ControllerLogicReturnValue ReturnValue = new ControllerLogicReturnValue();

            // try and get the Refresh Token cookie sent from the client (there may not be one there)
            var RefreshTokenCookie = httpRequest.Cookies.Where(x => x.Key == appSettings.RefreshTokenCookieName);

            // if the cookie was not found
            if (RefreshTokenCookie.Any() == false)
            {
                ReturnValue.HasErrors = true;
                ReturnValue.Errors.Add("Refresh Token Cookie not recieved");
                return ReturnValue;
            }

            // get the value stored in the cookie (this should be an encrypted Token
            ReturnValue.ReturnValue = RefreshTokenCookie.First().Value;

            return ReturnValue;
        }

        /// <summary>
        /// Looks for the encrypted token in the database and if found and valid, returns the dycrypted token
        /// </summary>
        /// <param name="EncryptedRefreshToken"></param>
        /// <param name="appSettings"></param>
        /// <param name="httpResponse"></param>
        /// <returns>will set HttpResponse to 401 if Invalid</returns>
        private ControllerLogicReturnValue DycryptRefreshToken(string EncryptedRefreshToken, AppSettings appSettings, HttpResponse httpResponse)
        {
            ControllerLogicReturnValue ReturnValue = new ControllerLogicReturnValue();

            TokenManager tokenManager = new TokenManager(appSettings.RefreshTokenAge, appSettings.TokenManagerDatabaseLocation);

            
            Token? token = tokenManager.ConvertEncryptedTokenToToken(EncryptedRefreshToken, appSettings.RefreshTokenEncryptionPhrase);
            if (token == null)
            {
                ReturnValue.HasErrors = true;
                ReturnValue.Errors.Add("Invalid Token");
                httpResponse.StatusCode = 401;//Unauthorized

                // delete the Refresh token cookie
                this.DeleteRefreshTokenCookie(appSettings, httpResponse);
                return ReturnValue;
            }

            // the token is invalid and can't be used.
            if(tokenManager.IsTokenValid(token) == false)
            {
                ReturnValue.HasErrors = true;
                ReturnValue.Errors.Add("Invalid Token");
                httpResponse.StatusCode = 401;//Unauthorized

                // delete the token in the database
                tokenManager.DeleteToken(token.TokenID);

                // delete the Refresh token cookie
                this.DeleteRefreshTokenCookie(appSettings, httpResponse);
                return ReturnValue;
            }

            // the token is valid so set it to the return value
            ReturnValue.ReturnValue = token;

            return ReturnValue;
        }

        /// <summary>
        /// Updates the Token to a new version and increases its expiration date. Also creates the Cookie on the httpResponse
        /// </summary>
        /// <param name="token"></param>
        /// <param name="appSettings"></param>
        /// <param name="httpResponse"></param>
        /// <returns>if sucsesfull <see cref="ControllerLogicReturnValue.ReturnValue"/> = (Token updatedToken, string EncryptedToken)</returns>
        private ControllerLogicReturnValue UpdateTokenToNewVersion(Token token, AppSettings appSettings, HttpResponse httpResponse)
        {
            ControllerLogicReturnValue ReturnValue = new ControllerLogicReturnValue();
            TokenManager tokenManager = new TokenManager(appSettings.RefreshTokenAge, appSettings.TokenManagerDatabaseLocation);

            (Token NewToken,string EncryptedToken)? TokenData = tokenManager.UpdateTokenToNextVersion(token, appSettings.RefreshTokenEncryptionPhrase);

            if(TokenData == null)
            {
                ReturnValue.HasErrors = true;
                ReturnValue.Errors.Add("Invalid Token");
                httpResponse.StatusCode = 401;//Unauthorized

                tokenManager.DeleteToken(token.TokenID);

                // delete the Refresh token cookie
                this.DeleteRefreshTokenCookie(appSettings, httpResponse);

                return ReturnValue;
            }


            // create a cookie and set its value to the refresh tokens encrypted data
            if (this.CreateCookie(appSettings.RefreshTokenCookieName,
                                  TokenData.Value.EncryptedToken,
                                  DateTimeOffset.UtcNow.AddMinutes(appSettings.RefreshTokenAge),
                                  httpResponse) == false)
            {
                // send back a 403 response
                ReturnValue.HasErrors = true;
                ReturnValue.Errors.Add("Cookie not created");
                httpResponse.StatusCode = 403;//The server understood the request, but cannot fulfill it.

                return ReturnValue;
            }


            // return the updated Token and its encrypted value
            ReturnValue.ReturnValue = (TokenData.Value.NewToken, TokenData.Value.EncryptedToken);

            return ReturnValue;
        }


        /// <summary>
        /// Creates a jwt string for the passed in user
        /// </summary>
        /// <param name="appSettings"></param>
        /// <param name="UserId"></param>
        /// <returns>String.Empty if unable to create else a JWT string</returns>
        private ControllerLogicReturnValue CreateJsonWebToken(AppSettings appSettings, int UserId, HttpResponse httpResponse)
        {
            ControllerLogicReturnValue ReturnValue = new ControllerLogicReturnValue();
            CandidateDetails? candidateDetailsModel;
            List<RolesAdminHas> rolesAdminHas;
            JwtCreator jwtCreator;

            // get the users details from the database
            candidateDetailsModel = this.GetUsersDetailsFromDatabase(UserId, appSettings);

            // if we did not get the user details
            if (candidateDetailsModel == null)
            {
                // send back a 403 response
                ReturnValue.HasErrors = true;
                ReturnValue.Errors.Add("jwt not created");
                httpResponse.StatusCode = 403;//The server understood the request, but cannot fulfill it.

                return ReturnValue;
            }

            // get all the roles this admin user has
            rolesAdminHas = this.GetAdminUsersRoles(UserId, appSettings);

            // inishalize the json web token createor, giving it the time the token expires
            jwtCreator = new JwtCreator(DateTime.UtcNow.AddMinutes(appSettings.jwtAge), appSettings.jwtSecretKey);


            // add candidates name to the jwt
            jwtCreator.AddClaim("name", $"{candidateDetailsModel.FirstName} {candidateDetailsModel.Surname}");
            // add the roles this user has
            jwtCreator.AddClaim("roles", string.Join(",", rolesAdminHas.Select(s => s.RoleName)));

            // generate the json web token string and add it to the return value
            ReturnValue.ReturnValue = jwtCreator.GenerateJwtString();
            return ReturnValue;


        }



        /// <summary>
        /// Get users details from the database
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="appSettings"></param>
        /// <returns></returns>
        private CandidateDetails GetUsersDetailsFromDatabase(int UserID, AppSettings appSettings)
        {
            SqLiteCon con;
            dbCandidates candidatesDB;
            CandidateDetails? candidateDetailsModel;

            con = new SqLiteCon();
            con.OpenConnection(appSettings.DataBaseLocation);

            candidatesDB = new dbCandidates(con);
            candidateDetailsModel = candidatesDB.Select(UserID);

            con.CloseConnection();

            return candidateDetailsModel;

        }

        /// <summary>
        /// Get a list of all roles the passed in user has
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="appSetting"></param>
        /// <returns></returns>
        private List<RolesAdminHas> GetAdminUsersRoles(int UserID, AppSettings appSetting)
        {
            SqLiteCon con;
            dbRolesAdminHas RolesAdminHasDb;
            List<RolesAdminHas> rolesAdminHas;

            con = new SqLiteCon();
            con.OpenConnection(appSetting.DataBaseLocation);

            RolesAdminHasDb = new dbRolesAdminHas(con);
            rolesAdminHas = RolesAdminHasDb.SelectRolesAdminHas(UserID);

            con.CloseConnection();

            return rolesAdminHas;

        }


        /// <summary>
        /// Creates a cookie to send back to the client
        /// </summary>
        /// <param name="CookieName"></param>
        /// <param name="CookieValue"></param>
        /// <param name="CookieExpiryDate"></param>
        /// <param name="httpResponse"></param>
        /// <returns></returns>
        private bool CreateCookie(string CookieName, string CookieValue, DateTimeOffset CookieExpiryDate, HttpResponse httpResponse)
        {

            httpResponse.Cookies.Append(CookieName, CookieValue,
                                        new CookieOptions
                                        {
                                            Path = "/API/Authorization/RefreshToken",
                                            Secure = true,
                                            HttpOnly = true,
                                            Expires = CookieExpiryDate,
                                            SameSite = SameSiteMode.None
                                        });

            return true;
        }


        /// <summary>
        /// Remove the Refresh token cookie from the HttpResponse
        /// </summary>
        /// <param name="appSettings"></param>
        /// <param name="httpResponse"></param>
        private void DeleteRefreshTokenCookie(AppSettings appSettings, HttpResponse httpResponse)
        {
            httpResponse.Cookies.Delete(appSettings.RefreshTokenCookieName);
        }

    }
}
