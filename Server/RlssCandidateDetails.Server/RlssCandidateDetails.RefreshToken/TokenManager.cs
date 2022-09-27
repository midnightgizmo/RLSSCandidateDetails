using RlssCandidateDetails.RefreshToken.Database;
using RlssCandidateDetails.RefreshToken.Models;
using RlssCandidateDetails.RefreshToken.Database.Tables;
using RlssCandidateDetails.RefreshToken.Database.TableModels;

namespace RlssCandidateDetails.RefreshToken
{
    public class TokenManager
    {
        private int _NumberOfMinutesBeforeTokenExpires;
        private string _DatabaseLocation;
        public TokenManager(int NumberOfMinutesBeforeTokenExpires, string DatabaseLocation)
        {
            this._NumberOfMinutesBeforeTokenExpires = NumberOfMinutesBeforeTokenExpires;
            this._DatabaseLocation = DatabaseLocation;
        }


        /// <summary>
        /// Creates a new token in the database
        /// </summary>
        /// <param name="TokenID">The ID the token will be given (this must be unique, call <see cref="CreateTokenID"/>)</param>
        /// <param name="CustomerID">The ID of the customer so we know who the Token belongs too</param>
        /// <returns>The new token or null if could not be created</returns>
        public (Token NewToken ,string EncryptedToken)? CreateNewToken(string TokenID, int CustomerID, string EncryptionPhrase)
        {
            Token? newToken;

            // If the token allready exists, return null to indicate we can't create the token
            if (this.DoesTokenExistInDatabase(TokenID) == true)
                return null;

            newToken = Token.CreateNewToken(TokenID,DateTime.UtcNow.AddMinutes(this._NumberOfMinutesBeforeTokenExpires), CustomerID);

            (string EncryptedData, string Base64SaltValue, string Base64IvValue)? encryptedData = this.EncryptToken(EncryptionPhrase, newToken);

            // if we were unable to encrypt the token
            if (encryptedData == null)
                return null;

            // create a hash of the base64Encrypted token
            string HashedEncryptedToken = Security.Encryption.HashStrting(encryptedData?.EncryptedData);

            // try and add the token to the database, returns null if fails.
            newToken = this.CreateNewTokenInDatabase(TokenID, 1,
                                                     DateTime.UtcNow.AddMinutes(this._NumberOfMinutesBeforeTokenExpires),
                                                     CustomerID,HashedEncryptedToken,
                                                     encryptedData?.Base64SaltValue,
                                                     encryptedData?.Base64IvValue);

            // if we were unable to create the new token in the datbase
            if (newToken == null)
                return null;
            // we were able to create a new tokekn in the database
            else
                return (newToken, encryptedData?.EncryptedData);
            
        }

        /*public void FindToken(string EncryptedToken)
        {
            dbSqliteConnection con = new dbSqliteConnection();
            dbTokenVersions TokenVersionsDb;
            TokenVersionsDatabaseModel? TokenVersionModel;

            con = new dbSqliteConnection();

            con.OpenConnection(this._DatabaseLocation);

            TokenVersionsDb = new dbTokenVersions(con);
            // Hash the encrypted token and try and find its hash value in the database 
            TokenVersionModel = TokenVersionsDb.Select_ByHashedValue(Security.Encryption.HashStrting(EncryptedToken));

            con.CloseConnection();

            // did not find the token in the database
            if(TokenVersionModel == null)
            {
                return null;
            }

            TokenVersionModel



        }*/

        /// <summary>
        /// Removes the token and all versions of it from the database
        /// </summary>
        /// <param name="TokenID">The token to look for and delete</param>
        public void DeleteToken(string TokenID)
        {
            dbSqliteConnection con = new dbSqliteConnection();
            con.OpenConnection(this._DatabaseLocation);

            dbRefreshToken dbRefreshToken = new dbRefreshToken(con);
            dbRefreshToken.Delete(TokenID);

            // close the connection to the datbase.
            con.CloseConnection();
        }


        /// <summary>
        /// Converts EncryptedToken to its hash value and searching for the hash value in database.
        /// If found, it will convert the EncryptedToken to an instance of Token. You still need
        /// to check its a valid token by calling <see cref="IsTokenValid(Token)"/>
        /// </summary>
        /// <param name="EncryptedToken">The encrypted token as a string</param>
        /// <param name="DecriptionPhrase">THe decription phrase used to decrypt the token</param>
        /// <returns>null if anything goes wrong else the decrypted token</returns>
        public Token? ConvertEncryptedTokenToToken(string EncryptedToken, string DecriptionPhrase)
        {

            // get the hash value of the EncryptedToken
            string HashedToken = Security.Encryption.HashStrting(EncryptedToken);

            //////////////////////////////////////////////
            // find the hash value in the database

            TokenVersionsDatabaseModel? TokenVersionModel;
            dbSqliteConnection con = new dbSqliteConnection();
            con.OpenConnection(this._DatabaseLocation);

            dbTokenVersions TokenVersionsDb = new dbTokenVersions(con);
            TokenVersionModel = TokenVersionsDb.Select_ByHashedValue(HashedToken);

            con.CloseConnection();

            // if we were unable to find the tokens version
            if (TokenVersionModel == null)
                return null;

            Token? DecryptedToken = this.DecryptToken(DecriptionPhrase, EncryptedToken, TokenVersionModel.Salt, TokenVersionModel.IV);

            if (DecryptedToken == null)
                return null;

            return DecryptedToken;

        }



        /// <summary>
        /// Gets an instance of Token based on the TokenID passed in.
        /// </summary>
        /// <param name="TokenID">The token id to look for in the database</param>
        /// <returns>Instance of Token or null if not found</returns>
        public Token? GetToken(string TokenID)
        {
            dbSqliteConnection con = new dbSqliteConnection();
            con.OpenConnection(this._DatabaseLocation);


            dbRefreshToken dbRefreshToken = new dbRefreshToken(con);
            RefreshTokenDatabaseModel? dbRefreshTokenModel = dbRefreshToken.Select(TokenID);

            // if we could not find the token id in the database
            if (dbRefreshTokenModel == null)
            {
                con.CloseConnection();
                return null;
            }

            Token? newToken = new Token();
            newToken.TokenID = dbRefreshTokenModel.TokenID;
            newToken.VersionNumber = dbRefreshTokenModel.CurrentVersionNumber;
            newToken.UtcExpiryDate = dbRefreshTokenModel.UtcExpiryDate;
            // close the connection to the datbase.
            con.CloseConnection();

            return newToken;
        }


        /// <summary>
        /// Checks if the passed in token is valid. Checks Does exist, is latest version, is still in date.
        /// Deletes the Token from the database if checks fail to pass
        /// </summary>
        /// <param name="token">Token to check</param>
        /// <returns>True if valid, else false</returns>
        public bool IsTokenValid(Token token)
        {
            // Does this token exist in the database
            if (this.DoesTokenExistInDatabase(token.TokenID) == false)
                return false;

            // Is this the latest version of the token
            if (this.IsMostUptodateToken(token) == false)
            {// we have an old token version number.

                // This is bad, it may be the token has been compromised
                // delete all version of this tokekn
                this.DeleteAllVersionsOfToken(token.TokenID);

                return false;
            }

            // is the token still in date (has the token expired)
            if (this.IsTokenInDate(token.TokenID) == false)
            {
                // the token has expired.
                // Delete all versions of this token
                this.DeleteAllVersionsOfToken(token.TokenID);

                return false;
            }


            // all above checks have passed, token is good
            return true;
        }


        /// <summary>
        /// Updates the Passed in tokens version number and expiry date. Returns a new instance of the updated token
        /// </summary>
        /// <param name="token">The token to update to the next version number</param>
        /// <param name="EncryptionPhrase">The phrase used to encrpt the token</param>
        /// <returns>New instance of the updated token & its encrypted value or null if fails</returns>
        public (Token NewToken, string EncryptedToken)? UpdateTokenToNextVersion(Token token, string EncryptionPhrase)
        {
            // check to see if the passed in token exists in the database.
            // if it does, return a new token with its version number and expiry date updated


            // get the token from the database (don't trust what has been passed in to exist)
            dbSqliteConnection con = new dbSqliteConnection();
            con.OpenConnection(this._DatabaseLocation);

            dbRefreshToken dbRefreshToken = new dbRefreshToken(con);
            RefreshTokenDatabaseModel? dbRefreshTokenModel = dbRefreshToken.Select(token.TokenID);

            // if we could not find the token in the database
            if (dbRefreshTokenModel == null)
            {
                con.CloseConnection();
                // return null to indicate token was not updated
                return null;
            }


            var updatedRefreshTokenDbModel = dbRefreshToken.Update(dbRefreshTokenModel.TokenID,
                                                     dbRefreshTokenModel.CurrentVersionNumber + 1,
                                                     DateTime.UtcNow.AddMinutes(this._NumberOfMinutesBeforeTokenExpires),
                                                     dbRefreshTokenModel.CustomerID);

            // if update failed in the database
            if (updatedRefreshTokenDbModel == null)
            {
                // role back the changes in the database
                dbRefreshToken.Update(dbRefreshTokenModel.TokenID,
                                                     dbRefreshTokenModel.CurrentVersionNumber,
                                                     dbRefreshTokenModel.UtcExpiryDate,
                                                     dbRefreshTokenModel.CustomerID);
                con.CloseConnection();
                return null;
            }

            // we updated the token in the database, copy of the values into a new Token instance
            Token updatedToken = new Token();
            updatedToken.TokenID = updatedRefreshTokenDbModel.TokenID;
            updatedToken.VersionNumber = updatedRefreshTokenDbModel.CurrentVersionNumber;
            updatedToken.UserID = updatedRefreshTokenDbModel.CustomerID;
            updatedToken.UtcExpiryDate = updatedRefreshTokenDbModel.UtcExpiryDate;


            // add the new version to the dbTokenVersions
            (string EncryptedData, string Base64SaltValue, string Base64IvValue)? encryptedData = this.EncryptToken(EncryptionPhrase, updatedToken);

            // if we were unable to encrypt the token
            if (encryptedData == null)
            {
                // role back the changes in the database
                dbRefreshToken.Update(dbRefreshTokenModel.TokenID,
                                                     dbRefreshTokenModel.CurrentVersionNumber,
                                                     dbRefreshTokenModel.UtcExpiryDate,
                                                     dbRefreshTokenModel.CustomerID);

                con.CloseConnection();
                return null;
            }

            // create a hash of the base64Encrypted token
            string HashedEncryptedToken = Security.Encryption.HashStrting(encryptedData?.EncryptedData);


            // Add a new version row for this token to the TokenVersions table
            dbTokenVersions dbVersion = new dbTokenVersions(con);
            var dbTokenModel = dbVersion.Insert(updatedToken.TokenID,
                                                updatedToken.VersionNumber,
                                                HashedEncryptedToken,
                                                encryptedData?.Base64SaltValue,
                                                encryptedData?.Base64IvValue);

            // if adding new row to TokenVersions failed
            if(dbTokenModel == null)
            {
                // role back the changes in the database
                dbRefreshToken.Update(dbRefreshTokenModel.TokenID,
                                                     dbRefreshTokenModel.CurrentVersionNumber,
                                                     dbRefreshTokenModel.UtcExpiryDate,
                                                     dbRefreshTokenModel.CustomerID);

                con.CloseConnection();
                return null;
            }

            // return the updated token
            return (updatedToken, encryptedData?.EncryptedData);


        }



        #region private methods

        #region Encryption & Decryption

        /// <summary>
        /// Encrypt the passed in data using the passed in Encryption Phrase
        /// </summary>
        /// <param name="EncryptionPhrase">The secrete string used to encrypt the data</param>
        /// <param name="token">The Token to be encrypted</param>
        /// <returns>null if encryption fails</returns>
        private (string Base64EncryptedData, string Base64SaltValue, string Base64IvValue)? EncryptToken(string EncryptionPhrase, Token token)
        {
            // convert this instance of the class to json
            string jsonText = System.Text.Json.JsonSerializer.Serialize(token);
            // Encrypt the data and get back the encrypted data alongwith the salt and IV values used to encrypt the data
            (string EncryptedData, string Base64SaltValue, string Base64IvValue)? encryptedData = Security.Encryption.Encrypt(jsonText, EncryptionPhrase, Security.EncryptionType.Enc256);

            return encryptedData;

        }
        /// <summary>
        /// Decrypts the passed in text and turns it into an instance of a Token class
        /// </summary>
        /// <param name="DecryptionKeyPhrase">the password used to decrypt the EncryptedToken</param>
        /// <param name="encryptedToken">The encryptedToken Text</param>
        /// <returns>Token or null if unable to decrypt</returns>
        private Token? DecryptToken(string DecryptionKeyPhrase, string encryptedToken, string Base64SaltValue, string Base64IvValue)
        {
            string jsonText = Security.Encryption.Decrypt(encryptedToken, DecryptionKeyPhrase, Base64SaltValue, Base64IvValue, Security.EncryptionType.Enc256);
            Token? token = null;
            try
            {
                token = System.Text.Json.JsonSerializer.Deserialize<Token>(jsonText);
            }
            catch (Exception ex)
            {

            }

            return token;

        }
        #endregion

        #region CreateNewToken Helper methods

        private bool DoesTokenExistInDatabase(string TokenID)
        {
            dbSqliteConnection con = new dbSqliteConnection();
            con.OpenConnection(this._DatabaseLocation);
            bool ReturnValue = true;

            // check to make sure token does not allready exist
            dbRefreshToken dbRefreshToken = new dbRefreshToken(con);
            if (dbRefreshToken.Select(TokenID) != null)
                ReturnValue = true;
            else
                ReturnValue = false;

            con.CloseConnection();

            return ReturnValue;
        }
        /// <summary>
        /// Create a new token in the database (inserts row into RefreshTable & TokenVersions Table)
        /// </summary>
        /// <param name="TokenID">ID of the new token</param>
        /// <param name="VersionNumber">Tokens version number</param>
        /// <param name="ExpiryDate">Date/Time token expires</param>
        /// <param name="CustomerID">The ID of the customer so we know who the token belongs too</param>
        /// <returns></returns>
        private Token? CreateNewTokenInDatabase(string TokenID, int VersionNumber, DateTime ExpiryDate, int CustomerID, string TokenHashedValue, string Salt, string IV)
        {
            dbSqliteConnection con = new dbSqliteConnection();
            con.OpenConnection(this._DatabaseLocation);


            dbRefreshToken dbRefreshToken = new dbRefreshToken(con);

            // create a new token with its version set to 1 and set its expiry date to (UTCNow + _NumberOfSecondsBeforeTokenExpires)
            RefreshTokenDatabaseModel? dbRefreshTokenModel = dbRefreshToken.Insert(TokenID, 1, ExpiryDate, CustomerID);
            // make sure the token was created, if it wasn't return null
            if (dbRefreshTokenModel == null)
            {
                con.CloseConnection();
                return null;
            }

            // create the version number for the token in the TokenVersions Table
            dbTokenVersions dbTokenVersions = new dbTokenVersions(con);
            if (dbTokenVersions.Insert(TokenID, VersionNumber, TokenHashedValue,Salt,IV) == null)
            {// for some reason we were not able to create the token version.

                // delete the token that was just created in the RefreshToken table
                dbRefreshToken.Delete(dbRefreshTokenModel.TokenID);

                // close the database connection.
                con.CloseConnection();

                // return null to indicate we were not able to create the new token
                return null;
            }
            // token has been created


            // create a new token and populate it with all the values for the token we just created
            Token? newToken = new Token();
            newToken.TokenID = dbRefreshTokenModel.TokenID;
            newToken.VersionNumber = dbRefreshTokenModel.CurrentVersionNumber;
            newToken.UtcExpiryDate = dbRefreshTokenModel.UtcExpiryDate;
            newToken.UserID = dbRefreshTokenModel.CustomerID;
            // close the connection to the datbase.
            con.CloseConnection();

            return newToken;


        }

        #endregion

        /// <summary>
        /// Checks the current tokens version number against the tokens version number in the database
        /// </summary>
        /// <param name="token">The Token to check against the database</param>
        /// <returns>True if latest version, else false</returns>
        private bool IsMostUptodateToken(Token token)
        {
            bool isMostUpdate = false;
            dbSqliteConnection con = new dbSqliteConnection();
            con.OpenConnection(this._DatabaseLocation);


            dbRefreshToken dbRefreshToken = new dbRefreshToken(con);
            RefreshTokenDatabaseModel? dbRefreshTokenModel = dbRefreshToken.Select(token.TokenID);

            if (dbRefreshTokenModel == null)
            {// we could not find the token in the database so return false

                con.CloseConnection();
                return false;

            }

            // check the token version number passed in against the one in the database
            if (token.VersionNumber == dbRefreshTokenModel.CurrentVersionNumber)
                // passed in token is most uptodate token
                isMostUpdate = true;
            else
                // passed in token is an older version
                isMostUpdate = false;

            con.CloseConnection();

            // return true if most uptodate, else false
            return isMostUpdate;
        }


        /// <summary>
        /// Removes all instances in the database where TokenID is found. Rmoves from RefreshToken table & TokenVersions table
        /// </summary>
        /// <param name="TokenID"></param>
        private void DeleteAllVersionsOfToken(string TokenID)
        {
            dbSqliteConnection con = new dbSqliteConnection();
            con.OpenConnection(this._DatabaseLocation);


            dbRefreshToken dbRefreshToken = new dbRefreshToken(con);
            // This should cause a cascade delete to the TokenVersions Table.
            dbRefreshToken.Delete(TokenID);

            con.CloseConnection();
        }

        /// <summary>
        /// Checks the passed in token id against the database to see if it expiry date is still in date.
        /// </summary>
        /// <param name="TokenID">The token to look for in the database</param>
        /// <returns>True if in date, else false</returns>
        private bool IsTokenInDate(string TokenID)
        {
            dbSqliteConnection con = new dbSqliteConnection();
            con.OpenConnection(this._DatabaseLocation);


            dbRefreshToken dbRefreshToken = new dbRefreshToken(con);
            RefreshTokenDatabaseModel? dbRefreshTokenModel = dbRefreshToken.Select(TokenID);

            if (dbRefreshTokenModel == null)
            {// we could not find the token in the database so return false

                con.CloseConnection();
                return false;
            }

            // we found the token, check the date to see if it has expired
            if (dbRefreshTokenModel.UtcExpiryDate < DateTime.UtcNow)
            {
                // token has expired
                con.CloseConnection();
                return false;
            }

            con.CloseConnection();

            // token is still in date.
            return true;
        }

        #endregion

    }
}