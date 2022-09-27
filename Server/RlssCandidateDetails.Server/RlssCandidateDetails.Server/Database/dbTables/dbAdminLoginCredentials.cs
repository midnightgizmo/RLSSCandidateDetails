using Microsoft.Data.Sqlite;
using RlssCandidateDetails.Server.Models.Admin;
using System.Text;

namespace RlssCandidateDetails.Server.Database.dbTables
{
    /// <summary>
    /// CRUD methods for <see cref="AdminLoginCredentials"/>
    /// </summary>
    public class dbAdminLoginCredentials
    {
        private SqLiteCon _con;

        /// <summary>
        /// Name of the table in the database 
        /// </summary>
        public string tableName = "AdminLoginCredentials";

        /// <summary>
        /// Inishalized the class ready for interacting with the database.
        /// </summary>
        /// <param name="connection">SqLiteCon to the database which should have allready been opened before passing in</param>
        public dbAdminLoginCredentials(SqLiteCon connection)
        {
            this._con = connection;// this should be an allready open connection
        }

        #region Select Methods
        public AdminLoginCredentials? Select(string UserName, string HashedPassword)
        {
            AdminLoginCredentials adminLoginCredentials = null;
            StringBuilder sb = new StringBuilder();
            SqliteDataReader rdr;

            sb.Append("SELECT Id, CandidatesId, UserName, Password ");
            sb.Append("FROM " + this.tableName + " ");
            sb.Append("WHERE UserName=:UserName AND Password=:Password;");

            List<SqliteParameter> parametersArray = new List<SqliteParameter>();
            SqliteParameter aParameter;

            // UserName
            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":UserName";
            aParameter.Value = UserName;
            aParameter.DbType = System.Data.DbType.String;

            parametersArray.Add(aParameter);


            // Password
            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":Password";
            aParameter.Value = HashedPassword;
            aParameter.DbType = System.Data.DbType.String;

            parametersArray.Add(aParameter);

            // execute the sql statment
            rdr = this._con.ExecuteParameterizedSelectCommand(sb.ToString(), parametersArray.ToArray());

            // if we found a row
            if (rdr.Read())
            {
                // convert the row to a adminLoginCredentials object
                adminLoginCredentials = this.GetRowData(rdr);


            }
            rdr.Close();

            // return the adminLoginCredentials details, or null if not found
            return adminLoginCredentials;

        }

        #endregion


        #region Private Methods
        /// <summary>
        /// Converts the database row into a <see cref="AdminLoginCredentials"/>
        /// </summary>
        /// <param name="rdr">The database row that contains AdminLoginCredentials</param>
        /// <returns></returns>
        private AdminLoginCredentials GetRowData(SqliteDataReader rdr)
        {
            AdminLoginCredentials adminLoginCredentials = new AdminLoginCredentials();

            adminLoginCredentials.Id = rdr.GetInt32(0);
            adminLoginCredentials.CandidatesId = rdr.GetInt32(1);
            adminLoginCredentials.UserName = rdr.GetString(2);
            adminLoginCredentials.Password = rdr.GetString(3);
    
            return adminLoginCredentials;
        }
        #endregion
    }
}
