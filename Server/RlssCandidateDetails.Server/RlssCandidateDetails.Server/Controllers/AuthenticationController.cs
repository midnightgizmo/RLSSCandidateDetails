using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RlssCandidateDetails.Server.ControllersLogic;
using RlssCandidateDetails.Server.ControllersLogic.Authentication;
using RlssCandidateDetails.Server.Models;

namespace RlssCandidateDetails.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        public AuthenticationController(IOptions<AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        public AppSettings appSettings { get; set; }

        [HttpPost]
        [Route("Login")]
        [Produces("application/json")]
        public ControllerLogicReturnValue Login([FromForm]string username, [FromForm]string password)
        {
            LoginControllerLogic controllerLogic = new LoginControllerLogic();
            
            return controllerLogic.Process(this.appSettings, this.Response, username, password);
        }

        public ControllerLogicReturnValue Logout()
        {// Can i remove the cookie from hear if the cookie is only set to be sent back on a specific path ?
            return null;
        }

        /// <summary>
        /// Updates the passed in refresh token cookie (assuming it is valid) and returns Json Web token
        /// </summary>
        /// <returns>Response containing Json Web Token or Login Required</returns>
        [HttpGet]
        [Route("RefreshToken")]
        [Produces("application/json")]
        public ControllerLogicReturnValue RefreshToken()
        {
            RefreshTokenControllerLogic controllerLogic = new RefreshTokenControllerLogic();

            return controllerLogic.Process(this.appSettings, this.Request, this.Response);
            
        }
    }
}
