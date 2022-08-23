using Microsoft.Data.Sqlite;

namespace RlssCandidateDetails.Server.Database
{
    public class SqLiteCon
    {
        private SqliteConnection _con;
        /// <summary>
        /// Atempts to open a connection to an SQLite database
        /// </summary>
        /// <param name="location">the physical path to the SQLite database file</param>
        /// <returns>true if sucsefull, else false</returns>
        public bool OpenConnection(string location)
        {
            try
            {
                string connectionString = $"Data Source = {location};";
                this._con = new SqliteConnection(connectionString);
                this._con.Open();

                return true;

            }
            catch (Exception e)
            {

                return false;
            }

        }


        /// <summary>
        /// Attemps to close the SQLite database connection 
        /// </summary>
        /// <returns>true if sucsefull, else false</returns>
        public bool CloseConnection()
        {
            try
            {
                this._con.Close();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }



        public SqliteDataReader ExecuteSelectCommand(string SQLCommand)
        {
            SqliteCommand sqliteCommand;

            sqliteCommand = this._con.CreateCommand();
            sqliteCommand.CommandText = SQLCommand;

            try
            {
                return sqliteCommand.ExecuteReader();
            }
            catch (Exception e)
            {
                return null;
            }

        }


        public SqliteDataReader ExecuteParameterizedSelectCommand(string SQLCommand, SqliteParameter[] parameters) //$parameters = SQLiteParameter[]
        {
            SqliteCommand sqliteCommand;

            sqliteCommand = this._con.CreateCommand();
            sqliteCommand.CommandText = SQLCommand;


            sqliteCommand.Parameters.AddRange(parameters);

            try
            {
                return sqliteCommand.ExecuteReader();
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public int ExecuteParameterizedNoneReader(string SQLCommand, SqliteParameter[] parameters)
        {
            SqliteCommand sqliteCommand;

            sqliteCommand = this._con.CreateCommand();
            sqliteCommand.CommandText = SQLCommand;


            sqliteCommand.Parameters.AddRange(parameters);

            try
            {
                return sqliteCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        public int ExecuteNoneReader(string SQLCommand)
        {
            SqliteCommand sqliteCommand;

            sqliteCommand = this._con.CreateCommand();
            sqliteCommand.CommandText = SQLCommand;

            try
            {
                return sqliteCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        public int Get_Last_Insert_Id()
        {
            SqliteCommand sqliteCommand;

            sqliteCommand = this._con.CreateCommand();
            sqliteCommand.CommandText = "select last_insert_rowid()";

            try
            {
                return (int)(long)sqliteCommand.ExecuteScalar();
            }
            catch (Exception e)
            {
                return -1;
            }
        }

    }
}
