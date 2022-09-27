namespace RlssCandidateDetails.RefreshToken.Database
{
    using Microsoft.Data.Sqlite;
    public class dbSqliteConnection
    {
        // connection to SqliteConnection
        private SqliteConnection _con;

        /// <summary>
        /// Opens a connection to the database
        /// </summary>
        /// <param name="ServerLocation">Location of database on disk</param>
        /// <returns>True if database was opened sucsefully</returns>
        public bool OpenConnection(string ServerLocation)
        {
            bool WasConnectionOpended = false;
            
            var builder = new SqliteConnectionStringBuilder
            {
                ConnectionString = $"Data Source = {ServerLocation};"
            };

            try
            {
                
                this._con = new SqliteConnection(builder.ConnectionString);
                this._con.Open();

                WasConnectionOpended = true;
            }
            catch { }

            return WasConnectionOpended;

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
            catch
            {
                return false;
            }

        }

        public SqliteDataReader ExecuteSelectCommand(string SQLCommand)
        {
            SqliteCommand SqliteCommand;

            SqliteCommand = this._con.CreateCommand();
            SqliteCommand.CommandText = SQLCommand;

            try
            {
                return SqliteCommand.ExecuteReader();
            }
            catch
            {
                return null;
            }

        }


        public SqliteDataReader ExecuteParameterizedSelectCommand(string SQLCommand, SqliteParameter[] parameters) //$parameters = SQLiteParameter[]
        {
            SqliteCommand SqliteCommand;


            SqliteCommand = this._con.CreateCommand();
            SqliteCommand.CommandText = SQLCommand;


            SqliteCommand.Parameters.AddRange(parameters);

            try
            {
                return SqliteCommand.ExecuteReader();
            }
            catch
            {
                return null;
            }
        }


        public int ExecuteParameterizedNoneReader(string SQLCommand, SqliteParameter[] parameters)
        {
            SqliteCommand SqliteCommand;

            SqliteCommand = this._con.CreateCommand();
            SqliteCommand.CommandText = SQLCommand;


            SqliteCommand.Parameters.AddRange(parameters);

            try
            {
                return SqliteCommand.ExecuteNonQuery();
            }
            catch
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
            SqliteCommand SqliteCommand;

            SqliteCommand = this._con.CreateCommand();
            SqliteCommand.CommandText = "SELECT last_insert_rowid();";

            try
            {
                return (int)(long)SqliteCommand.ExecuteScalar();
            }
            catch
            {
                return -1;
            }
        }
    }
}
