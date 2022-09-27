using Microsoft.Data.Sqlite;
using RlssCandidateDetails.RefreshToken.Database;
using RlssCandidateDetails.RefreshToken.Database.TableModels;
using System.Text;

namespace RlssCandidateDetails.RefreshToken.Database.Tables
{
    public class dbTokenVersions
    {
        /// <summary>
        /// Reference to an open connection that is passed in through the constructor
        /// </summary>
        private dbSqliteConnection _con;

        /// <summary>
        /// Name of the table in the database
        /// </summary>
        public readonly string TableName = "TokenVersions";

        /// <summary>
        /// Inishalized the class ready for interacting with the database.
        /// </summary>
        /// <param name="connection">dbSqliteConnection to the database which should have allready been opened before passing in</param>
        public dbTokenVersions(dbSqliteConnection connection)
        {
            this._con = connection;
        }


        #region Select functions

        public TokenVersionsDatabaseModel? SelectVersion(string TokenID, int VersionNumber)
        {
            SqliteDataReader rdr;
            StringBuilder sb = new StringBuilder();

            TokenVersionsDatabaseModel? aTokenVersion = null ;

            sb.Append("SELECT RefreshToken_TokenID, VersionNumber, HashedToken, Salt, IV FROM " + this.TableName + " ");
            sb.Append("WHERE RefreshToken_TokenID=:TokenID AND VersionNumber = :VersionNumber");

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
            aParameter.ParameterName = ":VersionNumber";
            aParameter.Value = VersionNumber;
            aParameter.DbType = System.Data.DbType.Int32;

            // add the parameter to the parameter array
            parametersArray.Add(aParameter);


            rdr = this._con.ExecuteParameterizedSelectCommand(sb.ToString(), parametersArray.ToArray());

            if(rdr.Read())
            {
                // get the row from the database
                aTokenVersion = this.GetRowData(rdr);
            }
            rdr.Close();

            return aTokenVersion;
        }
        /// <summary>
        /// Gets a single TokenVersion based on the HashedTokenValue sent in
        /// </summary>
        /// <param name="HashedTokenValue">the SHA256 Hash of the token</param>
        /// <returns>Null if token not found</returns>
        public TokenVersionsDatabaseModel? Select_ByHashedValue(string HashedTokenValue)
        {
            SqliteDataReader rdr;
            StringBuilder sb = new StringBuilder();

            TokenVersionsDatabaseModel? aTokenVersion = null;

            sb.Append("SELECT RefreshToken_TokenID, VersionNumber, HashedToken, Salt, IV FROM " + this.TableName + " ");
            sb.Append("WHERE HashedToken=:HashedToken;");

            // holds a list of parameters to insert into the sql query
            List<SqliteParameter> parametersArray = new List<SqliteParameter>();
            SqliteParameter aParameter;

            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":HashedToken";
            aParameter.Value = HashedTokenValue;
            aParameter.DbType = System.Data.DbType.String;

            // add the parameter to the parameter array
            parametersArray.Add(aParameter);


            rdr = this._con.ExecuteParameterizedSelectCommand(sb.ToString(), parametersArray.ToArray());

            if (rdr.Read())
            {
                // get the row from the database
                aTokenVersion = this.GetRowData(rdr);
            }
            rdr.Close();

            return aTokenVersion;
        }
        public List<TokenVersionsDatabaseModel> SelectAllVersionsForToken(string TokenID)
        {
            SqliteDataReader rdr;
            StringBuilder sb = new StringBuilder();

            List<TokenVersionsDatabaseModel> TokenVersionsList = new List<TokenVersionsDatabaseModel>();

            sb.Append("SELECT RefreshToken_TokenID, VersionNumber, HashedToken, Salt, IV FROM " + this.TableName + " ");
            sb.Append("WHERE RefreshToken_TokenID=:TokenID");

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



            // get each row
            while (rdr.Read())
            {
                // get the data from the row
                TokenVersionsDatabaseModel aTokenVersion = new TokenVersionsDatabaseModel();

                // get the row from the database
                aTokenVersion = this.GetRowData(rdr);

                // add the model class to the array
                TokenVersionsList.Add(aTokenVersion);
            }
            rdr.Close();

            // retuns a list of rows found in the database
            return TokenVersionsList;


        }
        #endregion


        #region Insert functions
        /// <summary>
        /// Insert a new row into the database
        /// </summary>
        /// <param name="TokenID">The Token ID in the parent table (RefreshToken)</param>
        /// <param name="VersionNumber">The version number of this token</param>
        /// <param name="HashedToken">SHA256 Hash of the token</param>
        /// <param name="SaltValue">The salt value used when encrypting/decyrpting the token</param>
        /// <param name="IvValue">THe IV value used when encrypting/decypting the token</param>
        /// <returns></returns>
        public TokenVersionsDatabaseModel? Insert(string TokenID, int VersionNumber, string HashedToken, string SaltValue, string IvValue)
        {
            // this is what will be returned. will be null if insert fails
            TokenVersionsDatabaseModel? aTokenVersion = null;

            StringBuilder sb = new StringBuilder();

            sb.Append($"INSERT INTO {this.TableName} ");
            sb.Append("(RefreshToken_TokenID,VersionNumber, HashedToken, Salt, IV) ");
            sb.Append("VALUES(");
            sb.Append(":TokenID, :VersionNumber, :HashedToken, :Salt, :IV");
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
            aParameter.ParameterName = ":VersionNumber";
            aParameter.Value = VersionNumber;
            aParameter.DbType = System.Data.DbType.Int32;

            // add the parameter to the parameter array
            parametersArray.Add(aParameter);



            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":HashedToken";
            aParameter.Value = HashedToken;
            aParameter.DbType = System.Data.DbType.String;

            // add the parameter to the parameter array
            parametersArray.Add(aParameter);



            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":Salt";
            aParameter.Value = SaltValue;
            aParameter.DbType = System.Data.DbType.String;

            // add the parameter to the parameter array
            parametersArray.Add(aParameter);




            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":IV";
            aParameter.Value = IvValue;
            aParameter.DbType = System.Data.DbType.String;

            // add the parameter to the parameter array
            parametersArray.Add(aParameter);




            // execute the sql statment
            int NumRowsEffected;
            NumRowsEffected = this._con.ExecuteParameterizedNoneReader(sb.ToString(), parametersArray.ToArray());

            // if greater than zero the row was inserted
            if (NumRowsEffected > 0)
            {
                // get the details from the database for the row that was just created
                aTokenVersion = this.SelectVersion(TokenID, VersionNumber);
            }

            // return the new inserted row, or null if it failed
            return aTokenVersion;


        }
        #endregion




        #region Delete Methods
        /// <summary>
        /// Deletes the row in the database
        /// </summary>
        /// <param name="TokenID">Token id to look for</param>
        /// <param name="VersionNumber"> Version number to look for</param>
        /// <returns>True if deleted else false</returns>
        public bool Delete(string TokenID, int VersionNumber)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"DELETE FROM {this.TableName} ");
            sb.Append("WHERE RefreshToken_TokenID = :TokenID AND VersionNumber = :VersionNumber");

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
            aParameter.ParameterName = ":VersionNumber";
            aParameter.Value = TokenID;
            aParameter.DbType = System.Data.DbType.Int32;

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
        #endregion

        #region Private Methods
        private TokenVersionsDatabaseModel GetRowData(SqliteDataReader rdr)
        {
            TokenVersionsDatabaseModel aTokenVersion = new TokenVersionsDatabaseModel();

            aTokenVersion.RefreshToken_TokenID = (string)rdr["RefreshToken_TokenID"];
            aTokenVersion.VersionNumber = (Int32)(Int64)rdr["VersionNumber"];
            aTokenVersion.HashedToken = (string)rdr["HashedToken"];
            aTokenVersion.Salt = (string)rdr["Salt"];
            aTokenVersion.IV = (string)rdr["IV"];

            return aTokenVersion;
        }
        #endregion
    }
}
