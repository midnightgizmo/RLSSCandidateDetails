//using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RlssCandidateDetails.Server.Attributes;
using RlssCandidateDetails.Server.ControllersLogic;
using RlssCandidateDetails.Server.ControllersLogic.Candidate;
using RlssCandidateDetails.Server.Models;
using RlssCandidateDetails.Server.Models.Candidate;

namespace RlssCandidateDetails.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CandidateController : ControllerBase
    {
        public CandidateController(IOptions<AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        public AppSettings appSettings { get; set; }


        [HttpGet]
        [AuthorizeAttribute(AuthorizationType= AuthorizeType.Admin)]
        [Route("GetAllUsers")]
        [Produces("application/json")]
        public ControllerLogicReturnValue GetAllUsers()
        {
            GetAllUsersControllerLogic controllerLogic;          

            controllerLogic = new GetAllUsersControllerLogic();
            // get a list of all candidates details within the database
            return controllerLogic.Process(this.appSettings);

            
        }

        [HttpPost]
        [AuthorizeAttribute(AuthorizationType = AuthorizeType.Admin)]
        [Route("FindCandidates")]
        [Produces("application/json")]
        public ControllerLogicReturnValue FindUsers([FromForm] CandidateDetails CandidateSearchCriteria)
        {
            FindUsersControllerLogic controllerLogic = new FindUsersControllerLogic();

            return controllerLogic.Process(this.appSettings, CandidateSearchCriteria);
        }

        [HttpPost]
        [AuthorizeAttribute(AuthorizationType = AuthorizeType.Admin)]
        [Route("GetCandidate")]
        [Produces("application/json")]
        public ControllerLogicReturnValue GetUser([FromForm]int Id)
        {
            GetUserControllerLogic controllerLogic = new GetUserControllerLogic();

            // get the candidate details from the database
            return controllerLogic.Process(this.appSettings, Id);
        }

        /// <summary>
        /// Inserts NewCandidate Details into database. Checks for error and if
        /// candidate allready exists in database
        /// </summary>
        /// <param name="NewCandidate"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeAttribute(AuthorizationType = AuthorizeType.Admin)]
        [Route("Insert")]
        [Produces("application/json")]
        public ControllerLogicReturnValue InsertUser([FromForm]CandidateDetails NewCandidate)
        {
            InsertUserControllerLogic controllerLogic = new InsertUserControllerLogic();
            return controllerLogic.Process(this.appSettings, NewCandidate);
        }

        /// <summary>
        /// Updades a candidates details
        /// </summary>
        /// <param name="UpdatedCandidate">The candidates updated deetails to apply to the database</param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeAttribute(AuthorizationType = AuthorizeType.Admin)]
        [Route("Update")]
        [Produces("application/json")]
        public ControllerLogicReturnValue EditUser([FromForm] CandidateDetails UpdatedCandidate)
        {
            UpdateUserControllerLogic controllerLogic = new UpdateUserControllerLogic();
            return controllerLogic.Process(this.appSettings, UpdatedCandidate);
        }


        /// <summary>
        /// Remove the candidate from the Database that matched the passed in ID
        /// </summary>
        /// <param name="Id">The id to use to look for the candidate to remove from the database</param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeAttribute(AuthorizationType = AuthorizeType.Admin)]
        [Route("Delete")]
        [Produces("application/json")]
        public ControllerLogicReturnValue Delete([FromForm]int Id)
        {
            DeleteUserControllerLogic controllerLogic = new DeleteUserControllerLogic();
            return controllerLogic.Process(this.appSettings, Id);
        }
    }
}
