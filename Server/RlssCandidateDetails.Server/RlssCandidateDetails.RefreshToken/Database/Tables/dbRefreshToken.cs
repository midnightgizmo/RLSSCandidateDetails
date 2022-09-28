using Microsoft.Data.Sqlite;
using RlssCandidateDetails.RefreshToken.Database;
using RlssCandidateDetails.RefreshToken.Database.TableModels;
using RlssCandidateDetails.RefreshToken.Models;
using System.Text;

namespace RlssCandidateDetails.RefreshToken.Database.Tables
{
    public class dbRefreshToken
    {
        /// <summary>
        /// Reference to an open connection that is passed in through the constructor
        /// </summary>
        private dbSqliteConnection _con;

        /// <summary>
        /// Name of the table in the database
        /// </summary>
        public readonly string TableName = "RefreshToken";

        /// <summary>
        /// Inishalized the class ready for interacting with the database.
        /// </summary>
        /// <param name="connection">dbSqliteConnection to the database which should have allready been opened before passing in</param>
        public dbRefreshToken(dbSqliteConnection connection)
        {
            this._con = connection;
        }

        #region select functions
        /// <summary>
        /// Select all rows from the database
        /// </summary>
        public List<RefreshTokenDatabaseModel> SelectAll()
        {
            dbSqliteConnection con;
            SqliteDataReader rdr;
            StringBuilder sb = new StringBuilder();

            List<RefreshTokenDatabaseModel> RefreshTokenList = new List<RefreshTokenDatabaseModel>();

            sb.Append("SELECT TokenID, CurrentVersionNumber, UtcExpiryDate, CustomerID FROM " + this.TableName);

            rdr = this._con.ExecuteSelectCommand(sb.ToString());

            // go through each row in the database and add to the RefreshTokenList
            while (rdr.Read())
            {
                RefreshTokenDatabaseModel refreshToken = new RefreshTokenDatabaseModel();

                refreshToken.TokenID = (string)rdr["TokenID"];
                refreshToken.CurrentVersionNumber = (int)rdr["CurrentVersionNumber"];
                refreshToken.UtcExpiryDate = RefreshTokenDatabaseModel.UnixTimeStampToDateTime((int)rdr["UtcExpiryDate"]);
                refreshToken.CustomerID = (int)rdr["CustomerID"];

                RefreshTokenList.Add(refreshToken);
            }

            rdr.Close();

            return RefreshTokenList;


        }

        /// <summary>
        /// Select a specific RefreshToken Row
        /// </summary>
        /// <param name="TokenID">The ID of the row to look for</param>
        /// <returns>Refresh Token row or null if not found</returns>
        public RefreshTokenDatabaseModel? Select(string TokenID)
        {
            dbSqliteConnection con;
            SqliteDataReader rdr;
            StringBuilder sb = new StringBuilder();

            RefreshTokenDatabaseModel refreshToken = null;

            sb.Append("SELECT TokenID, CurrentVersionNumber, UtcExpiryDate, CustomerID FROM " + this.TableName + " ");
            sb.Append("WHERE TokenID=:TokenID;");

            // holds a list of parameters to insert into the sql query
            List<SqliteParameter> parametersArray = new List<SqliteParameter>();
            SqliteParameter aParameter;

            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":TokenID";
            aParameter.Value = TokenID;
            aParameter.DbType = System.Data.DbType.String;

            // add the parameter to the parameter array
            parametersArray.Add(aParameter);


            rdr = this._con.ExecuteParameterizedSelectCommand(sb.ToString(), parametersArray.ToArray());


            if(rdr.Read())
            {
                refreshToken = new RefreshTokenDatabaseModel();

                refreshToken.TokenID = (string)rdr["TokenID"];
                refreshToken.CurrentVersionNumber = (int)(Int64)rdr["CurrentVersionNumber"];
                refreshToken.UtcExpiryDate = RefreshTokenDatabaseModel.UnixTimeStampToDateTime((int)(Int64)rdr["UtcExpiryDate"]);
                refreshToken.CustomerID = (int)(Int64)rdr["CustomerID"];

                rdr.Close();
            }
            

            return refreshToken;
        }

        #endregion


        #region Insert functions

        /// <summary>
        /// Insert a new Refresh Token row into the database
        /// </summary>
        /// <param name="TokenID">Must be a Unique id</param>
        /// <param name="CurrentVersion">current token version number being used</param>
        /// <param name="CustomerID">The ID of the customer this token belongs to</param>
        /// <returns>A model of the inserted row, or null if fails</returns>
        public RefreshTokenDatabaseModel? Insert(string TokenID, int CurrentVersion, DateTime UtcExpiryDate, int CustomerID)
        {
            // this is what will be returned. will be null if insert fails
            RefreshTokenDatabaseModel? refreshTokenDatabaseModel = null;

            StringBuilder sb = new StringBuilder();

            sb.Append($"INSERT INTO {this.TableName} ");
            sb.Append("(TokenID,CurrentVersionNumber, UTCExpiryDate, CustomerID) ");
            sb.Append("VALUES(");
            sb.Append(":TokenID, :CurrentVersion, :UTCExpiryDate, :CustomerID");
            sb.Append(");");



            // holds a list of parameters to insert into the sql query
            List<SqliteParameter> parametersArray = new List<SqliteParameter>();
            SqliteParameter aParameter;

            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":TokenID";
            aParameter.Value = TokenID;
            aParameter.DbType = System.Data.DbType.String;

            // add the parameter to the parameter array
            parametersArray.Add(aParameter);



            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":CurrentVersion";
            aParameter.Value = CurrentVersion;
            aParameter.DbType = System.Data.DbType.Int32;

            // add the parameter to the parameter array
            parametersArray.Add(aParameter);




            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":UTCExpiryDate";
            aParameter.Value = RefreshTokenDatabaseModel.ConvertDateTimeToUnixTimeStamp(UtcExpiryDate);
            aParameter.DbType = System.Data.DbType.Int32;

            // add the parameter to the parameter array
            parametersArray.Add(aParameter);




            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":CustomerID";
            aParameter.Value = CustomerID;
            aParameter.DbType = System.Data.DbType.Int32;

            // add the parameter to the parameter array
            parametersArray.Add(aParameter);


            

            // execute the sql statment
            int NumRowsEffected;
            NumRowsEffected = this._con.ExecuteParameterizedNoneReader(sb.ToString(), parametersArray.ToArray());

            // if greater than zero the row was inserted
            if (NumRowsEffected > 0)
            {
                // get the details from the database for the row that was just created
                refreshTokenDatabaseModel = this.Select(TokenID);
            }

            // return the new inserted row, or null if it failed
            return refreshTokenDatabaseModel;

        }

        #endregion


        #region Update Functions
        /// <summary>
        /// Update the CurrentVersion and UTCExpiryDate collumns in the database
        /// </summary>
        /// <param name="TokenID">The row to look for</param>
        /// <param name="NewCurrentVersion">The new value to update</param>
        /// <param name="UTCExpiryDate">The new expiry date</param>
        /// <param name="CustomerID">The new Customer ID</param>
        /// <returns>A new model with the updated value or null if fails</returns>
        public RefreshTokenDatabaseModel? Update(string TokenID, int NewCurrentVersion, DateTime UTCExpiryDate, int CustomerID)
        {
            // holds the data we will get from the database
            RefreshTokenDatabaseModel? refreshTokenDatabaseModel = null;
            StringBuilder sb = new StringBuilder();

            sb.Append($"UPDATE {this.TableName} ");
            sb.Append("SET ");
            sb.Append("CurrentVersionNumber=:NewCurrentVersion, ");
            sb.Append("UTCExpiryDate=:UTCExpiryDate, ");
            sb.Append("CustomerID=:CustomerID ");
            sb.Append("WHERE TokenID=:TokenID");



            // holds a list of parameters to insert into the sql query
            List<SqliteParameter> parametersArray = new List<SqliteParameter>();
            SqliteParameter aParameter;

            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":NewCurrentVersion";
            aParameter.Value = NewCurrentVersion;
            aParameter.DbType = System.Data.DbType.Int32;

            // add the parameter to the parameter array
            parametersArray.Add(aParameter);



            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":UTCExpiryDate";
            aParameter.Value = RefreshTokenDatabaseModel.ConvertDateTimeToUnixTimeStamp(UTCExpiryDate);
            aParameter.DbType = System.Data.DbType.Int32;

            // add the parameter to the parameter array
            parametersArray.Add(aParameter);




            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":TokenID";
            aParameter.Value = TokenID;
            aParameter.DbType = System.Data.DbType.String;

            // add the parameter to the parameter array
            parametersArray.Add(aParameter);


            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":CustomerID";
            aParameter.Value = CustomerID;
            aParameter.DbType = System.Data.DbType.Int32;

            // add the parameter to the parameter array
            parametersArray.Add(aParameter);

            


            // execute the sql statment
            int NumRowsEffected;
            NumRowsEffected = this._con.ExecuteParameterizedNoneReader(sb.ToString(), parametersArray.ToArray());

            // if greater than zero the row was inserted
            if (NumRowsEffected > 0)
            {
                // get the details for the database that was just created
                refreshTokenDatabaseModel = this.Select(TokenID);
            }

            // return the new inserted row, or null if it failed
            return refreshTokenDatabaseModel;
        }

        #endregion


        #region Delete Functions
        /// <summary>
        /// Deletes the Row in the database and also deletes all rows in TokenVersions table that have the same TokenID
        /// </summary>
        /// <param name="TokenID">The ID to look for to delete a specific row</param>
        /// <returns>true if deleted else false</returns>
        public bool Delete(string TokenID)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"DELETE FROM {this.TableName} ");
            sb.Append("WHERE TokenID = :TokenID");

            // holds a list of parameters to insert into the sql query
            List<SqliteParameter> parametersArray = new List<SqliteParameter>();
            SqliteParameter aParameter;

            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":TokenID";
            aParameter.Value = TokenID;
            aParameter.DbType = System.Data.DbType.String;

            // add the parameter to the parameter array
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

        /// <summary>
        /// Removes all tokens from the database that have an expiry date less than the passed in DateTime
        /// </summary>
        /// <param name="ExpiredDate">Remove all tokens that are less than this Value</param>
        public void DeleteExpiredTokens(DateTime ExpiredDate)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"DELETE FROM {this.TableName} ");
            sb.Append("WHERE UtcExpiryDate < :UtcExpiryDate");

            // holds a list of parameters to insert into the sql query
            List<SqliteParameter> parametersArray = new List<SqliteParameter>();
            SqliteParameter aParameter;

            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":UtcExpiryDate";
            aParameter.Value = RefreshTokenDatabaseModel.ConvertDateTimeToUnixTimeStamp(ExpiredDate);
            aParameter.DbType = System.Data.DbType.Int32;

            parametersArray.Add(aParameter);

            // execute the sql statment
            this._con.ExecuteParameterizedNoneReader(sb.ToString(), parametersArray.ToArray());

            return;
        }

        #endregion
    }
}
