using RlssCandidateDetails.Server.Database;
using RlssCandidateDetails.Server.Database.dbTables;
using RlssCandidateDetails.Server.Models;
using RlssCandidateDetails.Server.Models.Candidate;

namespace RlssCandidateDetails.Server.ControllersLogic.Candidate
{
    public class GetAllUsersControllerLogic
    {
        /// <summary>
        /// Gets a list of all candidats deatils within the database
        /// </summary>
        /// <param name="appSettings">Contains the location to the database</param>
        /// <returns>List of candidates, any errors that occured</returns>
        public ControllerLogicReturnValue Process(AppSettings appSettings)
        {
            ControllerLogicReturnValue returnValue = new ControllerLogicReturnValue();
            SqLiteCon sqlCon;
            List<CandidateDetails> CandidatesList;

            sqlCon = new SqLiteCon();
            // open connection to the database
            sqlCon.OpenConnection(appSettings.DataBaseLocation);

            dbCandidates CandidatesDB = new dbCandidates(sqlCon);
            // get all the candidates from the database
            CandidatesList = CandidatesDB.SelectAllCandidates();

            // close the database connection
            sqlCon.CloseConnection();


            // if something went wrong when getting all the candidates
            if (CandidatesList == null)
            {
                returnValue.HasErrors = true;
                returnValue.Errors.Add("Unable to get List of candidats");
            }
            else
                returnValue.ReturnValue = CandidatesList;

            return returnValue;
        }
    }
}
