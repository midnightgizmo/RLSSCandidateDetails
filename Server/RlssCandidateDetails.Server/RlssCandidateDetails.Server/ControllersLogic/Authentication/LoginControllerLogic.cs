using RlssCandidateDetails.JsonWebToken;
using RlssCandidateDetails.RefreshToken;
using RlssCandidateDetails.RefreshToken.Models;
using RlssCandidateDetails.Server.Database;
using RlssCandidateDetails.Server.Database.dbTables;
using RlssCandidateDetails.Server.Models;
using RlssCandidateDetails.Server.Models.Admin;
using RlssCandidateDetails.Server.Models.Candidate;

namespace RlssCandidateDetails.Server.ControllersLogic.Authentication
{
    /// <summary>
    /// Checks a users login details and if correct, returns a json web token string and refresh token cookie
    /// </summary>
    public class LoginControllerLogic
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appSettings"></param>
        /// <param name="httpResponse"></param>
        /// <param name="UserName"></param>
        /// <param name="Password"></param>
        /// <returns>returns 401 if incorrect login. returns 403 if anythting else goes wrong. Else returns 200 if correct login</returns>
        public ControllerLogicReturnValue Process(AppSettings appSettings, HttpResponse httpResponse, string UserName, string Password)
        {
            ControllerLogicReturnValue MethodResponse;
            AdminLoginCredentials adminLoginCredentials = new AdminLoginCredentials();
            adminLoginCredentials.UserName = UserName;
            adminLoginCredentials.Password = AdminLoginCredentials.HashPassword(Password,appSettings.PasswordSaltValue);


            // check the input values to see if they are ok
            MethodResponse = this.CheckInputForErrors(adminLoginCredentials);
            // if we found errors
            if (MethodResponse.HasErrors)
                return MethodResponse;

            // checks the login details against the database to see if they can be found.
            MethodResponse = this.CheckIfLoginDetailsAreCorrect(appSettings, httpResponse, adminLoginCredentials);
            
            // if we did not find login deatils in the database
            // returns a 401 response
            if(MethodResponse.HasErrors == true)
                return MethodResponse;

            // create the Json web token which will be sent back to the user
            ControllerLogicReturnValue JsonWebTokenResponse;
            JsonWebTokenResponse = this.CreateJsonWebToken(appSettings, 
                                                  ((AdminLoginCredentials)MethodResponse.ReturnValue).CandidatesId, 
                                                  httpResponse);
            
            // if we could not create the Json web token
            if (JsonWebTokenResponse.HasErrors == true)
                return JsonWebTokenResponse;

            // create the Refresh token cookie and JWT
            MethodResponse = this.CreateRefreshToken(appSettings, (AdminLoginCredentials)MethodResponse.ReturnValue, httpResponse);

            // if we were unable to create the cookie
            if (MethodResponse.HasErrors == true)
                return MethodResponse;


            // we created the json web token string to return to the user
            // and we have created the refresh token cookie to send back to the user
            return JsonWebTokenResponse;
        }

        

        /// <summary>
        /// Checks the Username and password properters of passed in variable to see if they are present
        /// and reports any erorrs it finds
        /// </summary>
        /// <param name="adminLoginCredentials"></param>
        /// <returns></returns>
        private ControllerLogicReturnValue CheckInputForErrors(AdminLoginCredentials adminLoginCredentials)
        {
            ControllerLogicReturnValue ReturnValue = new ControllerLogicReturnValue();
            
            // remove any whitespace at begining or end of username and password
            adminLoginCredentials.UserName = adminLoginCredentials?.UserName.Trim();
            adminLoginCredentials.Password = adminLoginCredentials?.Password.Trim();

            // check if we have recieved a username
            if(adminLoginCredentials.UserName == null || adminLoginCredentials.UserName.Length == 0)
            {
                ReturnValue.HasErrors = true;
                ReturnValue.Errors.Add("Username is required");
            }
            // check if we have recieved a password
            if(adminLoginCredentials.Password == null || adminLoginCredentials.Password.Length == 0)
            {
                ReturnValue.HasErrors = true;
                ReturnValue.Errors.Add("Password is required");
            }

            return ReturnValue;
        }

        /// <summary>
        /// Check the passed in login details against the database to see if they are correct
        /// </summary>
        /// <param name="appSettings"></param>
        /// <param name="httpResponse"></param>
        /// <param name="adminLoginCredentials"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private ControllerLogicReturnValue CheckIfLoginDetailsAreCorrect(AppSettings appSettings, HttpResponse httpResponse, AdminLoginCredentials adminLoginCredentials)
        {
            ControllerLogicReturnValue ReturnValue = new ControllerLogicReturnValue();

            SqLiteCon con;
            dbAdminLoginCredentials dbAdminLogin;
            AdminLoginCredentials? adminLoginDetailsFromDatabase;

            con = new SqLiteCon();

            con.OpenConnection(appSettings.DataBaseLocation);

            dbAdminLogin = new dbAdminLoginCredentials(con);
            adminLoginDetailsFromDatabase = dbAdminLogin.Select(adminLoginCredentials.UserName, adminLoginCredentials.Password);
            // if we could not find the login details in the database
            if (adminLoginDetailsFromDatabase == null)
            {
                ReturnValue.HasErrors = true;
                ReturnValue.Errors.Add("Invalid Username or Password");
                httpResponse.StatusCode = 401;//Unauthorized
            }
            else
                ReturnValue.ReturnValue = adminLoginDetailsFromDatabase;
            

            con.CloseConnection();



            return ReturnValue;
        }



        private ControllerLogicReturnValue CreateRefreshToken(AppSettings appSettings, AdminLoginCredentials adminLoginDetails, HttpResponse httpResponse)
        {
            ControllerLogicReturnValue ReturnValue = new ControllerLogicReturnValue();

            // set up the token manager ready to use
            TokenManager tokenManager = new TokenManager(appSettings.RefreshTokenAge, appSettings.TokenManagerDatabaseLocation);
            
            // create a new Token and get back the token and its string equivelent when encrypted 
            (Token? newToken, string EncryptedToken)? TokenData = tokenManager.CreateNewToken(Token.CreateTokenID(), adminLoginDetails.CandidatesId, appSettings.RefreshTokenEncryptionPhrase);
            
            // if we were unable to create a new Token
            if(TokenData == null)
            {
                // send back a 403 response
                ReturnValue.HasErrors = true;
                ReturnValue.Errors.Add("Token not created");
                httpResponse.StatusCode = 403;//The server understood the request, but cannot fulfill it.

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
        /// Creates a cookie to send back to the client
        /// </summary>
        /// <param name="CookieName"></param>
        /// <param name="CookieValue"></param>
        /// <param name="CookieExpiryDate"></param>
        /// <param name="httpResponse"></param>
        /// <returns></returns>
        private bool CreateCookie(string CookieName, string CookieValue, DateTimeOffset CookieExpiryDate, HttpResponse httpResponse)
        {

            httpResponse.Cookies.Append(CookieName,CookieValue,
                                        new CookieOptions
                                        {
                                            //Path = "API/Authorization/RefreshToken/",
                                            Secure = true,
                                            HttpOnly = true,
                                            Expires = CookieExpiryDate,
                                            SameSite = SameSiteMode.None
                                        });
            
            
            return true;
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






    }
}
