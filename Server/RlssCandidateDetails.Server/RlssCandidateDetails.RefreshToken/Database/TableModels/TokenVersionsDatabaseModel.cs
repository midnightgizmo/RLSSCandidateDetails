namespace RlssCandidateDetails.RefreshToken.Database.TableModels
{
    public class TokenVersionsDatabaseModel
    {

        public string RefreshToken_TokenID { get; set; } = string.Empty;
        public int VersionNumber { get; set; } = -1;
        /// <summary>
        /// This version hashed value (SHA256 Hash)
        /// </summary>
        public string HashedToken { get; set; } = string.Empty;
        /// <summary>
        /// The Salt value that was generated when encrypting the refresh token (in base64)
        /// </summary>
        public string Salt { get; set; } = string.Empty;
        /// <summary>
        /// The IV value that was generated when encrypting the refresh token (in base64)
        /// </summary>
        public String IV { get; set; } = String.Empty;
    }
}
