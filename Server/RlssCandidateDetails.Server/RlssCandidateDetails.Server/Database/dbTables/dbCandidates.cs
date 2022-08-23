using Microsoft.Data.Sqlite;
using RlssCandidateDetails.Server.Models.Candidate;
using System.Text;

namespace RlssCandidateDetails.Server.Database.dbTables
{
    /// <summary>
    /// CRUD methods for <see cref="CandidateDetails"/> in database
    /// </summary>
    public class dbCandidates
    {
        private SqLiteCon _con;

        /// <summary>
        /// Name of the table in the database 
        /// </summary>
        public string tableName = "Candidates";

        /// <summary>
        /// Inishalized the class ready for interacting with the database.
        /// </summary>
        /// <param name="connection">SqLiteCon to the database which should have allready been opened before passing in</param>
        public dbCandidates(SqLiteCon connection)
        {
            this._con = connection;// this should be an allready open connection
        }


        #region Select functions
        /// <summary>
        /// Get all CandidateDetails in the database
        /// </summary>
        /// <returns></returns>
        public List<CandidateDetails> SelectAllCandidates()
        {
            List<CandidateDetails> ListOfCandidates = new List<CandidateDetails>();
            StringBuilder sb = new StringBuilder();
            SqliteDataReader rdr;

            sb.Append("SELECT ID, FirstName, Surname, SocietyNumber, DateOfBirth ");
            sb.Append("FROM " + this.tableName + " ");
            sb.Append("ORDER BY FirstName, Surname;");

            // execute the sql statment
            rdr = this._con.ExecuteSelectCommand(sb.ToString());

            // go throug each row returned from the database
            while(rdr.Read())
            {
                // convert the row to a CandidateDetails object
                CandidateDetails aCandidate = this.GetRowData(rdr);

                // add the candidate to the ListOfCandidates
                ListOfCandidates.Add(aCandidate);

            }
            rdr.Close();

            // return allthe candidates we have found.
            return ListOfCandidates;
        }

        public CandidateDetails Select(int Id)
        {
            CandidateDetails CandidatesDetails = null;
            StringBuilder sb = new StringBuilder();
            SqliteDataReader rdr;

            sb.Append("SELECT ID, FirstName, Surname, SocietyNumber, DateOfBirth ");
            sb.Append("FROM " + this.tableName + " ");
            sb.Append("WHERE ID=:Id;");


            List<SqliteParameter> parametersArray = new List<SqliteParameter>();
            SqliteParameter aParameter;

            // First Aid
            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":Id";
            aParameter.Value = Id;
            aParameter.DbType = System.Data.DbType.Int32;

            parametersArray.Add(aParameter);


            // execute the sql statment
            rdr = this._con.ExecuteParameterizedSelectCommand(sb.ToString(), parametersArray.ToArray());

            // if we found a row
            if (rdr.Read())
            {
                // convert the row to a CandidateDetails object
                CandidatesDetails = this.GetRowData(rdr);


            }
            rdr.Close();

            // return the candidates details, or null if not found
            return CandidatesDetails;
        }

        /// <summary>
        /// Select all candidates who match the input parameters
        /// </summary>
        /// <param name="FirstName">look for candidates who have this first name</param>
        /// <param name="Surname">look for candidates who have this surname</param>
        /// <returns>List of all candidats found that match the input search</returns>
        public List<CandidateDetails> Select(string FirstName, string Surname)
        {
            List<CandidateDetails> ListOfCandidates = new List<CandidateDetails>();
            StringBuilder sb = new StringBuilder();
            SqliteDataReader rdr;

            sb.Append("SELECT ID, FirstName, Surname, SocietyNumber, DateOfBirth ");
            sb.Append("FROM " + this.tableName + " ");
            sb.Append("WHERE FirstName=:FirstName AND Surname=:Surname ");
            sb.Append("ORDER BY FirstName, Surname;");


            List<SqliteParameter> parametersArray = new List<SqliteParameter>();
            SqliteParameter aParameter;

            // First name
            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":FirstName";
            aParameter.Value = FirstName;
            aParameter.DbType = System.Data.DbType.String;

            parametersArray.Add(aParameter);

            // Surname
            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":Surname";
            aParameter.Value = Surname;
            aParameter.DbType = System.Data.DbType.String;

            parametersArray.Add(aParameter);


            // execute the sql statment
            rdr = this._con.ExecuteParameterizedSelectCommand(sb.ToString(), parametersArray.ToArray());

            // go through each row
            while (rdr.Read())
            {
                // convert the row to a CandidateDetails object
                CandidateDetails aCandidate = this.GetRowData(rdr);

                // add the candidate to the ListOfCandidates
                ListOfCandidates.Add(aCandidate);


            }
            rdr.Close();

            // return the candidates details, or null if not found
            return ListOfCandidates;
        }

        /// <summary>
        /// Find the candidate by there Society Number
        /// </summary>
        /// <param name="SocietyNumber">The search parameter to use to look for the candidate</param>
        /// <returns>null if not found else a CandidateDetails object</returns>
        public CandidateDetails Select (string SocietyNumber)
        {
            CandidateDetails CandidatesDetails = null;
            StringBuilder sb = new StringBuilder();
            SqliteDataReader rdr;

            sb.Append("SELECT ID, FirstName, Surname, SocietyNumber, DateOfBirth ");
            sb.Append("FROM " + this.tableName + " ");
            sb.Append("WHERE SocietyNumber=:SocietyNumber;");


            List<SqliteParameter> parametersArray = new List<SqliteParameter>();
            SqliteParameter aParameter;

            // Society Number
            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":SocietyNumber";
            aParameter.Value = SocietyNumber;
            aParameter.DbType = System.Data.DbType.String;

            parametersArray.Add(aParameter);




            // execute the sql statment
            rdr = this._con.ExecuteParameterizedSelectCommand(sb.ToString(), parametersArray.ToArray());

            // if we found some data
            if (rdr.Read())
            {
                // convert the row to a CandidateDetails object
                CandidatesDetails = this.GetRowData(rdr);

            }
            rdr.Close();

            // return the candidates details, or null if not found
            return CandidatesDetails;
        }

        #endregion

        #region Find
        public List<CandidateDetails> FindCandidatesMatchingSearch(int Id, string FirstName, string Surname, string SocietyNumber, DateTime? DateOfBirth)
        {
            List<CandidateDetails> ListOfCandidates = new List<CandidateDetails>();
            StringBuilder sb = new StringBuilder();
            StringBuilder sbWhereClause = new StringBuilder();
            SqliteDataReader rdr;

            sb.Append("SELECT ID, FirstName, Surname, SocietyNumber, DateOfBirth ");
            sb.Append("FROM " + this.tableName + " ");
            sb.Append("WHERE ");

            // construct the where statment based on the values sent in.
            // if any values are empty, null or zero, we don't want to use them in the search

            List<SqliteParameter> parametersArray = new List<SqliteParameter>();
            SqliteParameter aParameter;

            // Do we want to search using ID
            if (Id > 0)
            {
                sbWhereClause.Append("Id=:Id");

                // Id
                aParameter = new SqliteParameter();
                aParameter.ParameterName = ":Id";
                aParameter.Value = Id;
                aParameter.DbType = System.Data.DbType.Int32;

                parametersArray.Add(aParameter);
            }

            // Do we want to search using First Name
            if(FirstName.Length > 0)
            {
                if (sbWhereClause.Length > 0)
                    sbWhereClause.Append(" AND ");

                sbWhereClause.Append("FirstName Like :FirstName");

                // FirstName
                aParameter = new SqliteParameter();
                aParameter.ParameterName = ":FirstName";
                aParameter.Value = "%" + FirstName + "%";
                aParameter.DbType = System.Data.DbType.String;

                parametersArray.Add(aParameter);

            }

            // Do we want to search using Surname
            if (Surname.Length > 0)
            {
                if (sbWhereClause.Length > 0)
                    sbWhereClause.Append(" AND ");

                sbWhereClause.Append("Surname Like :Surname");

                // Surname
                aParameter = new SqliteParameter();
                aParameter.ParameterName = ":Surname";
                aParameter.Value = "%" + Surname + "%";
                aParameter.DbType = System.Data.DbType.String;

                parametersArray.Add(aParameter);

            }

            // Do we want to search using Society Number
            if (SocietyNumber.Length > 0)
            {
                if (sbWhereClause.Length > 0)
                    sbWhereClause.Append(" AND ");

                sbWhereClause.Append("SocietyNumber Like :SocietyNumber");

                // SocietyNumber
                aParameter = new SqliteParameter();
                aParameter.ParameterName = ":SocietyNumber";
                aParameter.Value = "%" + SocietyNumber + "%";
                aParameter.DbType = System.Data.DbType.String;

                parametersArray.Add(aParameter);

            }

            // do we want to search using date of birth
            if (DateOfBirth != null)
            {
                if (sbWhereClause.Length > 0)
                    sbWhereClause.Append(" AND ");

                sbWhereClause.Append("DateOfBirth = :DateOfBirth");

                string DateOfBirthAsString = DateOfBirth.GetValueOrDefault().ToString("YYYY-MM-dd HH:mm:ss.fff");
                // Date of Birth
                aParameter = new SqliteParameter();
                aParameter.ParameterName = ":DateOfBirth";
                aParameter.Value = DateOfBirthAsString;
                aParameter.DbType = System.Data.DbType.String;

                parametersArray.Add(aParameter);
            }

            // if we have somthing to search for
            if(sbWhereClause.Length > 0)
            {
                string sqlText = sb.ToString() + sbWhereClause.ToString();
                rdr = this._con.ExecuteParameterizedSelectCommand(sqlText, parametersArray.ToArray());

                // get each row
                while(rdr.Read())
                {
                    // convert the row to a CandidateDetails object
                    CandidateDetails aCandidate = this.GetRowData(rdr);

                    // add the candidate to the ListOfCandidates
                    ListOfCandidates.Add(aCandidate);
                }
                rdr.Close();
            }

            return ListOfCandidates;



        }
        #endregion

        #region Insert Functions

        public CandidateDetails Insert(string FirstName, string Surname, string SocietyNumber, DateTime? DateOfBirth)
        {
            // the return value
            CandidateDetails newCandidatesDetails = null;

            StringBuilder sb = new StringBuilder();
            sb.Append("INSERT INTO " + this.tableName);
            sb.Append("(");
            sb.Append("FirstName, Surname, SocietyNumber");
            if (DateOfBirth != null)
                sb.Append(", DateOfBirth");
            sb.Append(") ");
            sb.Append("VALUES (");
            sb.Append(":FirstName, :Surname, :SocietyNumber");
            if (DateOfBirth != null)
                sb.Append(", :DateOfBirth");
            sb.Append(");");


            List<SqliteParameter> parametersArray = new List<SqliteParameter>();
            SqliteParameter aParameter;

            // First Aid
            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":FirstName";
            aParameter.Value = FirstName;
            aParameter.DbType = System.Data.DbType.String;

            parametersArray.Add(aParameter);


            // Surname
            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":Surname";
            aParameter.Value = Surname;
            aParameter.DbType = System.Data.DbType.String;

            parametersArray.Add(aParameter);

            // Society Number
            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":SocietyNumber";
            aParameter.Value = SocietyNumber;
            aParameter.DbType = System.Data.DbType.String;

            parametersArray.Add(aParameter);

            if (DateOfBirth != null)
            {
                string DateOfBirthAsString = DateOfBirth.GetValueOrDefault().ToString("YYYY-MM-dd HH:mm:ss.fff");
                // Date of Birth
                aParameter = new SqliteParameter();
                aParameter.ParameterName = ":DateOfBirth";
                aParameter.Value = SocietyNumber;
                aParameter.DbType = System.Data.DbType.String;

                parametersArray.Add(aParameter);
            }


            // execute the sql statment
            int NumRowsEffected;
            NumRowsEffected = this._con.ExecuteParameterizedNoneReader(sb.ToString(), parametersArray.ToArray());

            // if greater than zero the row was inserted
            if (NumRowsEffected > 0)
            {
                // get the id of the just created row
                int lastInsertID = this._con.Get_Last_Insert_Id();
                // get the users details for the user that was just created
                newCandidatesDetails = this.Select(lastInsertID);
            }

            // the candidates deatils, or null if not created
            return newCandidatesDetails;
        }

        #endregion

        #region Update functions
        /// <summary>
        /// Updated the Candidates deatils
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="FirstName"></param>
        /// <param name="Surname"></param>
        /// <param name="SocietyNumber">String.Empty is accepted</param>
        /// <param name="DateOfBirth">pass in null if don't want it to be set</param>
        /// <returns>null if there was a problem else the updated <see cref="CandidateDetails"/></returns>
        public CandidateDetails Update(int Id, string FirstName, string Surname, string SocietyNumber, DateTime? DateOfBirth)
        {
            CandidateDetails updatedCandidateDetails = null;
            StringBuilder sb = new StringBuilder();

            sb.Append("UPDATE " + this.tableName + " ");
            sb.Append("SET ");
            sb.Append("FirstName=:FirstName, ");
            sb.Append("Surname=:Surname,");
            sb.Append("SocietyNumber=:SocietyNumber ");
            if(DateOfBirth != null)
                sb.Append(", DateOfBirth=:DateOfBirth");
            sb.Append("WHERE Id=:Id;");



            List<SqliteParameter> parametersArray = new List<SqliteParameter>();
            SqliteParameter aParameter;

            // First Aid
            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":FirstName";
            aParameter.Value = FirstName;
            aParameter.DbType = System.Data.DbType.String;

            parametersArray.Add(aParameter);


            // Surname
            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":Surname";
            aParameter.Value = Surname;
            aParameter.DbType = System.Data.DbType.String;

            parametersArray.Add(aParameter);

            // Society Number
            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":SocietyNumber";
            aParameter.Value = SocietyNumber;
            aParameter.DbType = System.Data.DbType.String;

            parametersArray.Add(aParameter);

            // Date of birth
            if (DateOfBirth != null)
            {
                string DateOfBirthAsString = DateOfBirth.GetValueOrDefault().ToString("YYYY-MM-dd HH:mm:ss.fff");
                // Date of Birth
                aParameter = new SqliteParameter();
                aParameter.ParameterName = ":DateOfBirth";
                aParameter.Value = SocietyNumber;
                aParameter.DbType = System.Data.DbType.String;

                parametersArray.Add(aParameter);
            }


            // Id
            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":Id";
            aParameter.Value = Id;
            aParameter.DbType = System.Data.DbType.Int32;

            parametersArray.Add(aParameter);

            // execute the sql statment
            int NumRowsEffected;
            NumRowsEffected = this._con.ExecuteParameterizedNoneReader(sb.ToString(), parametersArray.ToArray());

            // if greater than zero the row was Updated
            if (NumRowsEffected > 0)
            {
                // get the users details for the user that was just created
                updatedCandidateDetails = this.Select(Id);
            }

            return updatedCandidateDetails;
        }
        #endregion

        #region Delete Functions
        public bool Delete(int Id)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("DELETE FROM " + this.tableName + " ");
            sb.Append("WHERE Id=:Id");

            List<SqliteParameter> parametersArray = new List<SqliteParameter>();
            SqliteParameter aParameter;

            // First Aid
            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":Id";
            aParameter.Value = Id;
            aParameter.DbType = System.Data.DbType.Int32;

            parametersArray.Add(aParameter);

            // execute the sql statment
            int NumRowsEffected;
            NumRowsEffected = this._con.ExecuteParameterizedNoneReader(sb.ToString(), parametersArray.ToArray());

            // did the row get deleted
            if (NumRowsEffected > 0)
                // row got deleted
                return true;
            else
                // row did not get deleted
                return false;

        }
        #endregion





        #region Private Methods
        /// <summary>
        /// Converts the database row into a <see cref="CandidateDetails"/>
        /// </summary>
        /// <param name="rdr">The database row that contains a users details</param>
        /// <returns></returns>
        private CandidateDetails GetRowData(SqliteDataReader rdr)
        {
            CandidateDetails aCandidate = new CandidateDetails();

            aCandidate.ID = rdr.GetInt32(0);
            aCandidate.FirstName = rdr.GetString(1);
            aCandidate.Surname = rdr.GetString(2);
            aCandidate.SocietyNumber = rdr.GetString(3);
            // Date is allowed to be null. 
            if (rdr[4] != DBNull.Value)
                aCandidate.DateOfBirth = this.ConvertStringToDateTime(rdr.GetString(4));

            return aCandidate;
        }

        /// <summary>
        /// Converts a string to a DateTime. string format is YYYY-MM-DD HH:MM:SS.SSS
        /// </summary>
        /// <param name="DateAsString">A date/time in the format YYYY-MM-DD HH:MM:SS.SSS or string.empty to indicate no datetime present</param>
        /// <returns>A date time if convertions was sucsefull, else null</returns>
        private DateTime? ConvertStringToDateTime(string DateAsString)
        {
            // if the date is empty, there is no datetime
            if (DateAsString == string.Empty)
                return null;

            DateTime date;
            // try and convert string to a date time
            if(DateTime.TryParse(DateAsString, out date) == true)
            {
                return date;
            }
            else
            {
                return null;
            }
        }
        #endregion
    }
}
