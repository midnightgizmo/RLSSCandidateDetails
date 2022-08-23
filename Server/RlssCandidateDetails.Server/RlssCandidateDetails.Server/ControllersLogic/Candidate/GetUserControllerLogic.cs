using RlssCandidateDetails.Server.Database;
using RlssCandidateDetails.Server.Database.dbTables;
using RlssCandidateDetails.Server.Models;
using RlssCandidateDetails.Server.Models.Candidate;

namespace RlssCandidateDetails.Server.ControllersLogic.Candidate
{
    public class GetUserControllerLogic
    {
        public ControllerLogicReturnValue Process(AppSettings appSettings, int CandidateId)
        {
            ControllerLogicReturnValue DataToReturn;

            // check the input data for errors
            DataToReturn = this.CheckForInputErrors(CandidateId);

            // did we find any errors
            if (DataToReturn.HasErrors)
                return DataToReturn;

            // attempt to find the candidates details in database
            return this.SelectCandidateInDatabase(appSettings, CandidateId);
        }

        
        /// <summary>
        /// Checks the CandidateId to see if is a valid number
        /// </summary>
        /// <param name="CandidateId"></param>
        /// <returns></returns>
        private ControllerLogicReturnValue CheckForInputErrors(int CandidateId)
        {
            ControllerLogicReturnValue ReturnData = new ControllerLogicReturnValue();

            // if the candidate id is zero or less, then something is wrong
            if(CandidateId <= 0)
            {
                ReturnData.HasErrors = true;
                ReturnData.Errors.Add("Invalid Candidate Id");
            }

            return ReturnData;
        }


        private ControllerLogicReturnValue SelectCandidateInDatabase(AppSettings appSettings, int CandidateId)
        {
            SqLiteCon sqlCon;
            CandidateDetails candidateData = null;
            ControllerLogicReturnValue ReturnData = new ControllerLogicReturnValue();


            sqlCon = new SqLiteCon();
            // open a connection to the database
            sqlCon.OpenConnection(appSettings.DataBaseLocation);

            dbCandidates CandidatesDB = new dbCandidates(sqlCon);
            // get the candidates details from the database
            candidateData = CandidatesDB.Select(CandidateId);

            // close connection to the databse
            sqlCon.CloseConnection();

            // if we were unable to find the candidates details in the database
            if (candidateData == null)
            {
                ReturnData.HasErrors = true;
                ReturnData.Errors.Add("The Candidates details could not be found");
            }
            else
                // set the reutrn value to the candidate details we found from the database
                ReturnData.ReturnValue = candidateData;

            // return the data if we found any, else report the errors we found
            return ReturnData;
        }
    }
}
