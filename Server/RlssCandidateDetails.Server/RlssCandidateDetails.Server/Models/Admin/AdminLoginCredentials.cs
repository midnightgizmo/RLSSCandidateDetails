using System.Security.Cryptography;
using System.Text;

namespace RlssCandidateDetails.Server.Models.Admin
{
    public class AdminLoginCredentials
    {
        public int Id { get; set; } = 0;
        /// <summary>
        /// The person these Login credentials belong too
        /// </summary>
        public int CandidatesId { get; set; } = 0;
        /// <summary>
        /// Plain text 
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Hashed password
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// The roles this user has been given for access to certain parts of the site
        /// </summary>
        public Roles RolesUserGranted { get; set; } = new Roles();


        /// <summary>
        /// Convets plain text password to a SHA256 Hash
        /// </summary>
        /// <param name="PlainTextPassword">The plain text password to hash</param>
        /// <param name="salt">The value to add to the begining of the password to help against rainbow tables</param>
        /// <returns>SHA256 Hashed String</returns>
        public static string HashPassword(string PlainTextPassword, string salt)
        {
            byte[] hashedData;
            // add the salt to the begining of the password.
            // This helps again look up tables
            string saltedPassword = salt + PlainTextPassword;

            // convert the password to a hashed byte array
            using (HashAlgorithm algorithm = SHA256.Create())
                hashedData = algorithm.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));

            // convert the byte array to a hashed string
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashedData)
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
    }
}
