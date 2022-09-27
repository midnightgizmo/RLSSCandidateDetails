using Microsoft.Data.Sqlite;
using RlssCandidateDetails.Server.Models.Admin;
using System.Text;

namespace RlssCandidateDetails.Server.Database.dbTables
{
    /// <summary>
    /// CRUD methods for <see cref="RolesAdminHas"/> in database
    /// </summary>
    public class dbRolesAdminHas
    {
        private SqLiteCon _con;

        /// <summary>
        /// Name of the table in the database 
        /// </summary>
        public string tableName = "RolesAdminHas";

        /// <summary>
        /// Inishalized the class ready for interacting with the database.
        /// </summary>
        /// <param name="connection">SqLiteCon to the database which should have allready been opened before passing in</param>
        public dbRolesAdminHas(SqLiteCon connection)
        {
            this._con = connection;// this should be an allready open connection
        }

        #region Select Methods
        public List<RolesAdminHas> SelectRolesAdminHas(int UserID)
        {
            List<RolesAdminHas> ListOfRolesAdminHas = new List<RolesAdminHas>();
            StringBuilder sb = new StringBuilder();
            SqliteDataReader rdr;
            /*
            sb.Append($"SELECT {this.tableName}.AdminLoginCredentialsId, {this.tableName}.RolesId, {dbRoles.tableName}.Name ");
            sb.Append($"FROM {this.tableName} ");
            sb.Append($"INNER JOIN {dbRoles.tableName} ON ");
            sb.Append($"{this.tableName}.AdminLoginCredentialsId = {dbRoles.tableName}.Id ");
            sb.Append($"WHERE {dbRoles.tableName}.Id=:AdminLoginCredentialsId;");
            */

            sb.Append("SELECT AdminLoginCredentials.Id, Roles.Id, Roles.Name ");
            sb.Append("FROM Candidates  ");
            sb.Append("INNER JOIN AdminLoginCredentials ON ");
            sb.Append("AdminLoginCredentials.CandidatesId = Candidates.Id ");
            sb.Append("INNER JOIN RolesAdminHas ON ");
            sb.Append("RolesAdminHas.AdminLoginCredentialsId = AdminLoginCredentials.Id ");
            sb.Append("INNER JOIN Roles ON ");
            sb.Append("Roles.Id = RolesAdminHas.RolesId ");
            sb.Append("WHERE Candidates.Id=:UserID;");
    
            List<SqliteParameter> parametersArray = new List<SqliteParameter>();
            SqliteParameter aParameter;

            // UserID
            aParameter = new SqliteParameter();
            aParameter.ParameterName = ":UserID";
            aParameter.Value = UserID;
            aParameter.DbType = System.Data.DbType.Int32;

            parametersArray.Add(aParameter);


            // execute the sql statment
            rdr = this._con.ExecuteParameterizedSelectCommand(sb.ToString(), parametersArray.ToArray());

            // go throug each row returned from the database
            while (rdr.Read())
            {
                // convert the row to a RolesAdminHas object
                RolesAdminHas aRoleAdminhas = this.GetRowData(rdr);

                // add the role to the ListOfRolesAdminHas
                ListOfRolesAdminHas.Add(aRoleAdminhas);

            }
            rdr.Close();

            // return all the roles we have found for admin user.
            return ListOfRolesAdminHas;

        }

        #endregion

        #region Private methods
        /// <summary>
        /// Converts the database row into a <see cref="RolesAdminHas"/>
        /// </summary>
        /// <param name="rdr">The database row that contains a admins RolesAdminHas</param>
        /// <returns></returns>
        private RolesAdminHas GetRowData(SqliteDataReader rdr)
        {
            RolesAdminHas aRole = new RolesAdminHas();

            aRole.AdminLoginCredentialsId = rdr.GetInt32(0);
            aRole.RolesId = rdr.GetInt32(1);
            aRole.RoleName = rdr.GetString(2);

            return aRole;
        }
        #endregion
    }
}
