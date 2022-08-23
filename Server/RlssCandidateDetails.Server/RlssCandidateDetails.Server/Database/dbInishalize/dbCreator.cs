using RlssCandidateDetails.Server.Database;

namespace RlssCandidateDetails.Server.Database.dbInishalize
{
    public class dbCreator
    {
        private const string _FileName = "dbInishalizationCode.txt";
        private const string _RelativeFolderLocation = "Database\\dbInishalize";
        
        /// <summary>
        /// Check if the database exists in the passed in location
        /// </summary>
        /// <param name="location">Location of where database should be</param>
        /// <returns>true if database was found, else false</returns>
        public bool DoesDataBaseExist(string location)
        {
            return System.IO.File.Exists(location);
            
        }

        /// <summary>
        /// Creates the database using the dbInishalizationCode.txt (does not check for an allready
        /// created database at passed in location)
        /// </summary>
        /// <param name="location">database name and location</param>
        /// <returns>true if created, else false</returns>
        public bool CreateDatabase(string location)
        {
            string path = System.IO.Directory.GetCurrentDirectory();
            path = System.IO.Path.Combine(path, _RelativeFolderLocation);
            path = System.IO.Path.Combine(path, dbCreator._FileName);
            
            try
            {
                string FileContent = string.Empty;
                FileContent = System.IO.File.ReadAllText(path);

                SqLiteCon connection = new SqLiteCon();
                connection.OpenConnection(location);
                connection.ExecuteNoneReader(FileContent);
                connection.CloseConnection();
            }
            catch
            {
                return false;
            }

            return true;
            
        }
    }
}
