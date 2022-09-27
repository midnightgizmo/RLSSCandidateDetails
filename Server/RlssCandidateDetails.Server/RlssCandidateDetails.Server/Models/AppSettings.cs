namespace RlssCandidateDetails.Server.Models
{
    public class AppSettings
    {

        public string DataBaseLocation { get; set; }

        /// <summary>
        /// Value to add to password to prevent Rainbow table lookups
        /// </summary>
        public string PasswordSaltValue { get; set; } = string.Empty;
        public string TokenManagerDatabaseLocation { get; set; }
        /// <summary>
        /// The cookie name for the Refresh Token.
        /// </summary>
        public string RefreshTokenCookieName { get; set; } = "RT";
        /// <summary>
        /// Number of Minutes to set refresh token into the future that it will expire
        /// </summary>
        /// 
        public int RefreshTokenAge { get; set; }
        /// <summary>
        /// The password to encrypted and dycrpt the RefreshToken
        /// </summary>
        public string RefreshTokenEncryptionPhrase { get; set; }

        /// <summary>
        /// The phrase used to sign the Json Web Token
        /// </summary>
        public string jwtSecretKey { get; set; }
        /// <summary>
        /// The number of minutes to set the Json Web Token into the fiture for when it will expire
        /// </summary>
        public int jwtAge { get; set; } = 2;
    }
}
