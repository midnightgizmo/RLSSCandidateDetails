using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RlssCandidateDetails.JsonWebToken
{
    /// <summary>
    /// Parses a Json web token string and validates it using the SecreteKey
    /// </summary>
   public class JwtParser
   {
        private Dictionary<string, object> _HeaderKeyValues;
        private Dictionary<string, object> _PayloadKeyValues;

        /// <summary>
        /// Checks if the jwt string is from us and in date.
        /// </summary>
        /// <param name="jwt">Json web token string to check</param>
        /// <param name="SecreteKey">The secrete key that was used when creating the json web token</param>
        /// <returns>true if valid, else false</returns>
        public static bool IsValidJWT(string jwt, string SecreteKey)
        {
            bool isValid = false;

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            // convert the key to a byte array 
            byte[] key = Encoding.ASCII.GetBytes(SecreteKey);
            // set the parameters for validating the jwt
            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero
            };

            // try and validate the token, if it fails it will through an execption
            try
            {
                // try and validate the token
                tokenHandler.ValidateToken(jwt, tokenValidationParameters, out SecurityToken validatedToken);

                // validated jwt
                JwtSecurityToken jwtToken = null;
                // set the return valid to the valid jwt
                jwtToken = (JwtSecurityToken)validatedToken;

                // set isValid to true to indicate we have a valid jwt
                isValid = true;

            }
            catch (SecurityTokenExpiredException e)
            {//token 'exp' claim is < DateTime.UtcNow.

            }
            catch (Exception e)
            {
                // do nothing if jwt validation fails
            }

            // returns true if a valid jwt, else false
            return isValid;
        }

        /// <summary>
        /// Call this after <see cref="JwtParser.ParseJWT(string)"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Claim> GetHeaderKeyValues()
        {
            return this._HeaderKeyValues.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
        }

        

        /// <summary>
        /// Call this after <see cref="JwtParser.ParseJWT(string)"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Claim> GetPayloadKeyValues()
        {
            return this._PayloadKeyValues.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
        }

        /// <summary>
        /// Returns the value asoshiated with with the passed in key name from the Payload section of the JWT
        /// </summary>
        /// <param name="key"></param>
        /// <returns>returns String.Empty if key not found</returns>
        public string GetPayloadValue(string key)
        {
            object keyValue;
            if (this._PayloadKeyValues.TryGetValue(key, out keyValue) == true)
            {
                
                return keyValue.ToString();
            }
            else
                return string.Empty;

            /*
            // if the key exists in the Dictionary
            if (this._PayloadKeyValues.ContainsKey(key) == true)
                return (string)this._PayloadKeyValues[key];
            // key not found in dictionary
            else
                return string.Empty;
            */

        }

        /// <summary>
        /// Looks for the exp key and checks to see if its date is in the future
        /// </summary>
        /// <returns>true is in date, else false</returns>
        public bool IsJsonWebTokenInDate()
        {
            string ExpirationDate = this.GetPayloadValue("exp");
            // if we could not find the expiration date
            if (ExpirationDate == string.Empty)
                return false;

            // try and convert the unix time stamp as a string to a long
            long UnixTimeStamp = 0;
            if (long.TryParse(ExpirationDate, out UnixTimeStamp) == false)
                return false;

            // if the time stamp is in the past, the jwt has expired
            if (UnixTimeStamp < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                return false;

            // we have a valid expiration date ( a date in the future)
            return true;
        }


        /// <summary>
        /// Converts the jwtString into an <see cref="JwtParser"/> object
        /// </summary>
        /// <param name="jwtString">json web token string</param>
        /// <returns>returns null if fails to parse</returns>
        public static JwtParser? ParseJWT(string jwtString)
        {
            JwtParser jwt = new JwtParser();
            // split the jwt into the 3 parts (Header, payload, signagure)
            string[] splitJwt = jwt.SplitToken(jwtString);

            if (splitJwt.Length != 3)
                return null;//new List<Claim>();

            // get the Header key value pairs
            jwt._HeaderKeyValues = jwt.GetKeyValueFromBase64(splitJwt[0]);
            // Get the Payload Key value pairs
            jwt._PayloadKeyValues = jwt.GetKeyValueFromBase64(splitJwt[1]);

            return jwt;
        }


        /// <summary>
        /// Splints the JWT into Header, Payload & Signature
        /// </summary>
        /// <param name="jwt">The Json web token to split up</param>
        /// <returns>Empty string[] if fails, else string[]{Header,Payload,Signature}</returns>
        private string[] SplitToken(string jwt)
        {
            // remove any whitespace from begining and end of string
            string value = jwt.Trim();

            // if the jwt is empty, return an empty string array to indicate error
            if (value == string.Empty)
                return new string[] { };

            // split the jwt at the '.'
            string[] jwtValuesArray = value.Split(new char[] { '.' });
            // if we did not get 3 sections after the split, we have an error
            if (jwtValuesArray.Length != 3)
                return new string[] { };

            // return the 3 sections as an array [Header,Payload, Signature]
            return new string[] { jwtValuesArray[0], jwtValuesArray[1], jwtValuesArray[2] };

        }

        /// <summary>
        /// Get Key value pairs from the passed in base64 string
        /// </summary>
        /// <param name="base64"></param>
        /// <returns>returns empty Dictinary if fails to convert</returns>
        private Dictionary<string, object> GetKeyValueFromBase64(string base64)
        {
            

            // convert the base64 string into a byte array
            byte[] data = this.ParseBase64WithoutPadding(base64);

            //string text = Encoding.UTF8.GetString(data);

            // convert the byte array into key value paris
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(data);

            // if the conversion failed
            if (keyValuePairs == null)
                return new Dictionary<string, object> { };
            // if the conversion sucseeded
            else
                return keyValuePairs;
        }

        /// <summary>
        /// Converts base64 string into byte array
        /// </summary>
        /// <param name="base64"></param>
        /// <returns></returns>
        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
