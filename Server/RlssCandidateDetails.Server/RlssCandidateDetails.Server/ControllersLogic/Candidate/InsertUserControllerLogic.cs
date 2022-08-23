using RlssCandidateDetails.Server.Database;
using RlssCandidateDetails.Server.Database.dbTables;
using RlssCandidateDetails.Server.Models;
using RlssCandidateDetails.Server.Models.Candidate;

namespace RlssCandidateDetails.Server.ControllersLogic.Candidate
{
    public class InsertUserControllerLogic
    {
        public ControllerLogicReturnValue Process(AppSettings appSettings, CandidateDetails CandidateToInsert)
        {
            
            ControllerLogicReturnValue DataToReturn;

            // check the candidate data for errors
            DataToReturn = this.CheckForErrors(CandidateToInsert);

            // did we find any errors
            if(DataToReturn.HasErrors)
                return DataToReturn;


            // attempt to insert the candidate into the database
            return this.InsertCandidateIntoDatabase(appSettings, CandidateToInsert);
                        
        }
        /// <summary>
        /// Check the passsed in data for any errors.
        /// </summary>
        /// <param name="CandidateToInsert">Data to check</param>
        /// <returns>ControllerLogicReturnValue.HasErrors will be true if errors found</returns>
        private ControllerLogicReturnValue CheckForErrors(CandidateDetails CandidateToInsert)
        {
            ControllerLogicReturnValue ReturnData = new ControllerLogicReturnValue();

            if (CandidateToInsert == null)
            {
                ReturnData.HasErrors = true;
                ReturnData.Errors.Add("No Data recieved");
                return ReturnData;
            }

            // remove any white space from begining and end of the strings
            CandidateToInsert.FirstName = CandidateToInsert.FirstName.Trim();
            CandidateToInsert.Surname = CandidateToInsert.Surname.Trim();
            CandidateToInsert.SocietyNumber = CandidateToInsert.SocietyNumber.Trim();

            
            // check if we have a first name
            if (CandidateToInsert.FirstName == null || CandidateToInsert.FirstName.Trim().Length == 0)
            {
                ReturnData.HasErrors = true;
                ReturnData.Errors.Add("First Name Required");  
            }

            // check if we have a surname
            if (CandidateToInsert.Surname == null || CandidateToInsert.Surname.Trim().Length == 0)
            {
                ReturnData.HasErrors = true;
                ReturnData.Errors.Add("Surname Required");
            }

            // check if we have a society number it is a valid number
            if(this.IsValidSocietyNumber(CandidateToInsert.SocietyNumber) == false)
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
            foreach(char num in SocietyNumber)
            {
                int number;
                if (int.TryParse(num.ToString(), out number) == false)
                    return false;
            }

            return true;

            
        }

        private ControllerLogicReturnValue InsertCandidateIntoDatabase(AppSettings appSettings, CandidateDetails CandidateToInsert)
        {
            SqLiteCon sqlCon;
            CandidateDetails NewCandidateDetails = null;
            ControllerLogicReturnValue ReturnValue;

            // check if the candidate we want to insert into the database
            // allready exists in the database.
            if(this.DoesCandidateAllreadyExist(appSettings, CandidateToInsert))
            {
                ReturnValue = new ControllerLogicReturnValue();
                ReturnValue.HasErrors = true;
                ReturnValue.Errors.Add("Candidate allready exists");

                return ReturnValue;
            }

            sqlCon = new SqLiteCon();
            // open connection to the database
            sqlCon.OpenConnection(appSettings.DataBaseLocation);

            dbCandidates CandidatesDB = new dbCandidates(sqlCon);
            // get all the candidates from the database
            NewCandidateDetails = CandidatesDB.Insert(CandidateToInsert.FirstName,CandidateToInsert.Surname,
                                                      CandidateToInsert.SocietyNumber,CandidateToInsert.DateOfBirth);

            // close the database connection
            sqlCon.CloseConnection();

            ReturnValue = new ControllerLogicReturnValue();
            ReturnValue.ReturnValue = NewCandidateDetails;
            if (NewCandidateDetails == null)
            {
                ReturnValue.HasErrors = true;
                ReturnValue.Errors.Add("Unable to add Candidate");
            }

            return ReturnValue;
        }

        /// <summary>
        /// Checks to see if the candidat allreay exists in the database. Will use Society Number if avalable,
        /// else will use first name and surname
        /// </summary>
        /// <param name="appSettings">Contains the location of the databsae</param>
        /// <param name="CandidateDetails">Details to use to look for candidate in database</param>
        /// <returns>True if exists in database, else false</returns>
        private bool DoesCandidateAllreadyExist(AppSettings appSettings, CandidateDetails CandidateDetails)
        {
            SqLiteCon sqlCon;
            bool WasCandidateFound = false;


            sqlCon = new SqLiteCon();
            // open connection to the database
            sqlCon.OpenConnection(appSettings.DataBaseLocation);

            dbCandidates CandidatesDB = new dbCandidates(sqlCon);

            // if we have a Society Number, use that to search for the candidate
            if (CandidateDetails.SocietyNumber.Length > 0)
            {
                CandidateDetails FoundCandidateDetails = null;
                FoundCandidateDetails = CandidatesDB.Select(CandidateDetails.SocietyNumber);
                // we found a candidate in the database that matches the Society Number
                if (FoundCandidateDetails != null)
                    WasCandidateFound = true;
            }
            // search for the candidate useing there First name and surname
            else
            {
                // if we find one or more matches in the database using first name and surname
                if (CandidatesDB.Select(CandidateDetails.FirstName, CandidateDetails.Surname).Count > 0)
                    WasCandidateFound = true;
            }
            // close connection to the database
            sqlCon.CloseConnection();


            return WasCandidateFound;
        }
    }
}
