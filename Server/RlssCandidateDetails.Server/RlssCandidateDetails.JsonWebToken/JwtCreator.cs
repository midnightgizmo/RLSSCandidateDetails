using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

namespace RlssCandidateDetails.JsonWebToken
{
    /// <summary>
    /// Creates a JWT and signs it with the SecreteKey passed in
    /// </summary>
    public class JwtCreator
    {
        private JwtSecurityTokenHandler _jwt;
        private SecurityTokenDescriptor _TokenDescriptor;

        /// <summary>
        /// Creates a new Jeson Web Token ready to add roles too.
        /// </summary>
        public JwtCreator(DateTime DateWhenExpires, string SecreteKey)
        {
            this._jwt = new JwtSecurityTokenHandler();
            // Inishalizes the SecurityTokenDescriptor and sets a few details, included expiry date
            // and SigningCredentials
            this.InishalizeTokenDescriptor(DateWhenExpires, SecreteKey);
        }

        public void AddClaim(string Key, object Value)
        {
            this._TokenDescriptor.Claims.Add(Key, Value);
        }

        /// <summary>
        /// Turn the JWT into "Header.Payload.Signature"
        /// </summary>
        /// <returns></returns>
        public string GenerateJwtString()
        {
            // create the jwt
            SecurityToken token = this._jwt.CreateToken(this._TokenDescriptor);

            // convert jwt to string and return it
            return this._jwt.WriteToken(token);
        }

        

        #region Private methods
        /// <summary>
        /// Inishalizes the <see cref="_TokenDescriptor"/> and sets the Expires, NotBefore, IssuedAt & 
        /// SigningCredentials
        /// </summary>
        /// <param name="JwtExpirtyDate">Date jwt should expire</param>
        /// <param name="JwtSecretKey"></param>
        private void InishalizeTokenDescriptor(DateTime JwtExpirtyDate, string JwtSecretKey)
        {
            // convert the secrete key to a byte array
            byte[] key = Encoding.ASCII.GetBytes(JwtSecretKey);

            // create the payload
            this._TokenDescriptor = new SecurityTokenDescriptor()
            {
                Expires = JwtExpirtyDate,
                NotBefore = DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 30)),//dateWhenExpires.Subtract(new TimeSpan(0, 0, 30)),
                IssuedAt = DateTime.UtcNow,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            this._TokenDescriptor.Claims = new Dictionary<string, object>();
        }



        #endregion
    }
}