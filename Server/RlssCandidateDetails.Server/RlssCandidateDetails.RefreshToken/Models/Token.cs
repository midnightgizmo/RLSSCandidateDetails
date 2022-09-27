using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlssCandidateDetails.RefreshToken.Models
{
    /// <summary>
    /// A refresh token that can be passed between client and server by converting it too and from a Json Web Token
    /// </summary>
    public class Token
    {
        public string TokenID { get; set; } = string.Empty;
        public int VersionNumber { get; set; } = -1;
        /// <summary>
        /// The date/time the refresh token expiers (this is in Universal Time)
        /// </summary>
        public DateTime UtcExpiryDate { get; set; }

        public int UserID { get; set; } = -1;

        /// <summary>
        /// Converts this instance of the class to a Json WebToken
        /// </summary>
        /// <returns>A Json web token string</returns>
        public string ConvertToJsonWebToken()
        {
            return string.Empty;
        }

        /// <summary>
        /// Takes in a JsonWebToken and attempts to convert it to a <see cref="Token"/> of this instance of the class
        /// </summary>
        /// <param name="JsonWebToken">JsonWebToken to convert</param>
        /// <returns>True if was converted, else false</returns>
        public bool PareJsonWebToken(string JsonWebToken)
        {
            return false;
        }

        /// <summary>
        /// Creates a new token with its version number set to 1
        /// </summary>
        /// <param name="ExpiryDateUTC">date the token expires</param>
        /// <param name="customerID">ID of the customer this token belongs too</param>
        /// <returns></returns>
        public static Token? CreateNewToken(string TokenID, DateTime ExpiryDateUTC, int customerID)
        {
            Token newToken = new Token();
            newToken.TokenID = TokenID;
            newToken.VersionNumber = 1;
            newToken.UserID = customerID;
            newToken.UtcExpiryDate = ExpiryDateUTC;

            return newToken;
        }



        /// <summary>
        /// Creates a unique Token ID to use for creating a new token
        /// </summary>
        /// <returns>Token ID</returns>
        public static string CreateTokenID()
        {
            Guid tokenID = Guid.NewGuid();
            return tokenID.ToString();

        }
    }
}
