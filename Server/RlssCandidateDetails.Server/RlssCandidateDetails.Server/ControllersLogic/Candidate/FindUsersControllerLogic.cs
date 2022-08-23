using RlssCandidateDetails.Server.Database;
using RlssCandidateDetails.Server.Database.dbTables;
using RlssCandidateDetails.Server.Models;
using RlssCandidateDetails.Server.Models.Candidate;

namespace RlssCandidateDetails.Server.ControllersLogic.Candidate
{
    public class FindUsersControllerLogic
    {
        public ControllerLogicReturnValue Process(AppSettings appSettings, CandidateDetails CandidateSearchCriteria)
        {
            ControllerLogicReturnValue ReturnValue;

            ReturnValue = this.CheckInput(CandidateSearchCriteria);
            
            if(ReturnValue.HasErrors)
            {
                return ReturnValue;
            }

            ReturnValue = this.FindCandidates(appSettings, CandidateSearchCriteria);

            return ReturnValue;
        }

        

        private ControllerLogicReturnValue CheckInput(CandidateDetails candidateSearchCriteria)
        {
            ControllerLogicReturnValue ReturnValue = new ControllerLogicReturnValue();

            candidateSearchCriteria.FirstName = candidateSearchCriteria.FirstName == null ? string.Empty : candidateSearchCriteria.FirstName.Trim();
            candidateSearchCriteria.Surname = candidateSearchCriteria.Surname == null ? string.Empty : candidateSearchCriteria.Surname.Trim();
            candidateSearchCriteria.SocietyNumber = candidateSearchCriteria.SocietyNumber == null ? string.Empty : candidateSearchCriteria.SocietyNumber.Trim();

            if(!CandidateDetails.IsValidSocietyNumber(candidateSearchCriteria.SocietyNumber))
            {
                ReturnValue.HasErrors = true;
                ReturnValue.Errors.Add("Invalid Society Number");
            }

            return ReturnValue;
        }

        

        private ControllerLogicReturnValue FindCandidates(AppSettings appSettings, CandidateDetails candidateSearchCriteria)
        {
            ControllerLogicReturnValue ReturnValue = new ControllerLogicReturnValue();
            List<CandidateDetails> ListOfFoundCandidates = new List<CandidateDetails>();
            SqLiteCon con = new SqLiteCon();

            con.OpenConnection(appSettings.DataBaseLocation);
            dbCandidates SqliteCandidates = new dbCandidates(con);

            // look for any candidates that match the search criteria we are passing in.
            // a like '%%' will be done on first name, surname & society number.
            ListOfFoundCandidates = SqliteCandidates.FindCandidatesMatchingSearch(candidateSearchCriteria.ID,
                                                                                  candidateSearchCriteria.FirstName,
                                                                                  candidateSearchCriteria.Surname,
                                                                                  candidateSearchCriteria.SocietyNumber,
                                                                                  candidateSearchCriteria.DateOfBirth);
            con.CloseConnection();

            ReturnValue.ReturnValue = ListOfFoundCandidates;

            return ReturnValue;
        }
    }
}
