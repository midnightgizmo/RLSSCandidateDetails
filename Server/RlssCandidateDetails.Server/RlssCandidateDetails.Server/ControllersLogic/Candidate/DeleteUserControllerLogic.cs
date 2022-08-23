using RlssCandidateDetails.Server.Database;
using RlssCandidateDetails.Server.Database.dbTables;
using RlssCandidateDetails.Server.Models;

namespace RlssCandidateDetails.Server.ControllersLogic.Candidate
{
    public class DeleteUserControllerLogic
    {
        public ControllerLogicReturnValue Process(AppSettings appSettings, int CandidateId)
        {
            ControllerLogicReturnValue returnValue = new ControllerLogicReturnValue();
            SqLiteCon sqlCon;
            dbCandidates CandidatesDB;
            bool WasCandidateDelete;

            sqlCon = new SqLiteCon();

            sqlCon.OpenConnection(appSettings.DataBaseLocation);

            CandidatesDB = new dbCandidates(sqlCon);
            WasCandidateDelete = CandidatesDB.Delete(CandidateId);

            if (!WasCandidateDelete)
            {
                returnValue.HasErrors = true;
                returnValue.Errors.Add("Unable to remove Candidate. Candidate might not exist.");
            }

            sqlCon.CloseConnection();

            // set to true if candidate was deleted, else set to false
            returnValue.ReturnValue = WasCandidateDelete;

            return returnValue;

        }
    }
}
