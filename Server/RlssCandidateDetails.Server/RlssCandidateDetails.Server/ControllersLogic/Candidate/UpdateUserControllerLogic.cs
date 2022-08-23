using RlssCandidateDetails.Server.Database;
using RlssCandidateDetails.Server.Database.dbTables;
using RlssCandidateDetails.Server.Models;
using RlssCandidateDetails.Server.Models.Candidate;

namespace RlssCandidateDetails.Server.ControllersLogic.Candidate
{
    public class UpdateUserControllerLogic
    {
        public ControllerLogicReturnValue Process(AppSettings appSettings, CandidateDetails CandidateToUpdate)
        {
            ControllerLogicReturnValue DataToReturn;

            // check the candidate data for errors
            DataToReturn = this.CheckForErrors(CandidateToUpdate);

            // did we find any errors
            if (DataToReturn.HasErrors)
                return DataToReturn;


            // attempt to update the candidates details in database
            return this.InsertCandidateIntoDatabase(appSettings, CandidateToUpdate);
        }

        

        private ControllerLogicReturnValue CheckForErrors(CandidateDetails candidateToUpdate)
        {
            ControllerLogicReturnValue ReturnData = new ControllerLogicReturnValue();

            if (candidateToUpdate == null)
            {
                ReturnData.HasErrors = true;
                ReturnData.Errors.Add("No Data recieved");
                return ReturnData;
            }

            // remove any white space from begining and end of the strings
            candidateToUpdate.FirstName = candidateToUpdate.FirstName.Trim();
            candidateToUpdate.Surname = candidateToUpdate.Surname.Trim();
            candidateToUpdate.SocietyNumber = candidateToUpdate.SocietyNumber.Trim();

            // check we have a valid Id
            if (candidateToUpdate.ID <= 0)
            {
                ReturnData.HasErrors = true;
                ReturnData.Errors.Add("Invalid Id");
            }

            // check if we have a first name
            if (candidateToUpdate.FirstName.Length == 0)
            {
                ReturnData.HasErrors = true;
                ReturnData.Errors.Add("First Name Required");
            }

            // check if we have a surname
            if (candidateToUpdate.Surname.Length == 0)
            {
                ReturnData.HasErrors = true;
                ReturnData.Errors.Add("Surname Required");
            }

            // check if we have a society number it is a valid number
            if (this.IsValidSocietyNumber(candidateToUpdate.SocietyNumber) == false)
            {
                ReturnData.HasErrors = true;
                ReturnData.Errors.Add("Invalid Society Number");
            }

            return ReturnData;
        }

        /// <summary>
        /// Check each char in the society number to see if its a number
        /// </summary>
        /// <param name="SocietyNumber">value to check to see if its a number</param>
        /// <returns>true if SocietyNumber is empty or a number. false if not a number </returns>
        private bool IsValidSocietyNumber(string SocietyNumber)
        {
            // check each char in the string to make sure it is a number
            foreach (char num in SocietyNumber)
            {
                int number;
                if (int.TryParse(num.ToString(), out number) == false)
                    return false;
            }

            return true;


        }

        private ControllerLogicReturnValue InsertCandidateIntoDatabase(AppSettings appSettings, CandidateDetails candidateToUpdate)
        {
            SqLiteCon sqlCon;
            CandidateDetails updatedCandidatesDetails = null;
            ControllerLogicReturnValue returnValue = new ControllerLogicReturnValue();


            sqlCon = new SqLiteCon();
            // open connection to the database
            sqlCon.OpenConnection(appSettings.DataBaseLocation);

            dbCandidates CandidatesDB = new dbCandidates(sqlCon);

            updatedCandidatesDetails = CandidatesDB.Update(candidateToUpdate.ID, candidateToUpdate.FirstName, candidateToUpdate.Surname, candidateToUpdate.SocietyNumber, candidateToUpdate.DateOfBirth);
            
            // if we were unable to update candidates details
            if(updatedCandidatesDetails == null)
            {
                returnValue.HasErrors = true;
                returnValue.Errors.Add("Unable to updated Candidates details");
            }
            // Candidates deatils were updated.
            else
            {
                // set the updated candidates details as the value we want to send back to the user
                returnValue.ReturnValue = updatedCandidatesDetails;
            }

            // close connection to the database
            sqlCon.CloseConnection();


            return returnValue;
        }


    }
}
