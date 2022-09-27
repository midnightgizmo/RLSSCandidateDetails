using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RlssCandidateDetails.RefreshToken.Security
{

    public enum EncryptionType { Enc128, Enc192, Enc256 };
    public class Encryption
    {
        private static int SaltValueSize = 32;
        // The higher the number the better but also the slow it will be
        private static int InterationCount = 1000;




        /// <summary>
        /// Uses a password to create a private key and then encrypts the passed in data
        /// </summary>
        /// <param name="TextToEncrypt">The string to encrypt</param>
        /// <param name="passPhrase">The password to use to encrypt the data</param>
        /// <param name="encryptionStrength">The private key size</param>
        /// <returns>The Encrypted data or String.Empty if anything went wrong</returns>
        public static (string EncryptedData, string Base64SaltValue, string Base64IvValue)? Encrypt(string TextToEncrypt, string passPhrase, EncryptionType encryptionStrength)
        {
            try
            {
                byte[] SaltValue;

                // create some random numbers. This will be used allong with the password string passed in
                // to create a private key
                SaltValue = Encryption.GenerateSaltValue(Encryption.SaltValueSize);

                // Turns the plane text password into a private encryption key
                Rfc2898DeriveBytes PrivateKey = new Rfc2898DeriveBytes(passPhrase, SaltValue, Encryption.InterationCount);


                // Encrypt the data.
                Aes encAlg = Aes.Create();
                // 16 = 128 bit encryption. 24 = 192 bit encryption. 32 = 256 bit encryption.
                // default is 16 to pass in (128 bit encryption). Might need to change key size if want to use any other size of encryption
                //encAlg.Key = PrivateKey.GetBytes(16);
                switch (encryptionStrength)
                {
                    case EncryptionType.Enc128:
                        encAlg.KeySize = 128;
                        encAlg.Key = PrivateKey.GetBytes(16);

                        break;

                    case EncryptionType.Enc192:
                        encAlg.KeySize = 192;
                        encAlg.Key = PrivateKey.GetBytes(24);

                        break;

                    case EncryptionType.Enc256:
                        encAlg.KeySize = 256;
                        encAlg.Key = PrivateKey.GetBytes(32);

                        break;

                }
                MemoryStream encryptionStream = new MemoryStream();
                CryptoStream encrypt = new CryptoStream(encryptionStream, encAlg.CreateEncryptor(), CryptoStreamMode.Write);
                // convert the unencrypted text to a byte array
                byte[] utfD1 = new System.Text.UTF8Encoding(false).GetBytes(TextToEncrypt);

                // write the unencrypted data into the Crypto Stream (this is where it will get encrypted)
                encrypt.Write(utfD1, 0, utfD1.Length);
                encrypt.FlushFinalBlock();
                encrypt.Close();
                // get back the encrypted data as a byte array
                byte[] EncryptedDataInByteArray = encryptionStream.ToArray();
                PrivateKey.Reset();


                // Convert the byte array to a base 64 value and add the salt and iv values into the data.
                //
                // add the randomly generated salt value & IV to the begining of the ecrypted data
                // then convert the hole thing to base64.
                // The salt value & IV are needed when we want to decrypt the data, so we need to store them somewhere.
                // Good place might be the database, but we are going to put it in with the encrypted data.
                // While not a good idea the user still can't decrypt the data without the password
                // and they would need to know we stored the salt value at the begining of the encrypted data.
                //return Convert.ToBase64String(SaltValue.Concat(encAlg.IV).Concat(EncryptedDataInByteArray).ToArray());

                // return the encrypted data, the salt value and the IV value
                return (
                        Convert.ToBase64String(EncryptedDataInByteArray),
                        Convert.ToBase64String(SaltValue),
                        Convert.ToBase64String(encAlg.IV)
                        );
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public static string Decrypt(string TextToDecrypt, string passPhrase, string Salt, string IV, EncryptionType encryptionStrength)
        {
            try
            {
                // convert the passed in data to a byte array.
                // The Iv size was done when data was being encrypted. It is the size of the blocks when they are encrypted
                // It is normaly 16 is basicaly Aes.BlockSize / 8
                int IVSize = 16;
                byte[] SaltIVAndEncryptedData = Convert.FromBase64String(TextToDecrypt);
                // get the salt value as a byte array
                byte[] SaltValue = Convert.FromBase64String(Salt);
                // get the IV Value as a byte array
                byte[] IVValue = Convert.FromBase64String(IV);
                // get the encrypted data as a byte array
                byte[] EncryptedData = Convert.FromBase64String(TextToDecrypt);

                // turn the plain text password into a private key
                Rfc2898DeriveBytes PrivateKey = new Rfc2898DeriveBytes(passPhrase, SaltValue);

                Aes decAlg = Aes.Create();
                //decAlg.Key = PrivateKey.GetBytes(16);
                // the the key size we will be  using
                switch (encryptionStrength)
                {
                    case EncryptionType.Enc128:
                        decAlg.KeySize = 128;
                        decAlg.Key = PrivateKey.GetBytes(16);

                        break;

                    case EncryptionType.Enc192:
                        decAlg.KeySize = 192;
                        decAlg.Key = PrivateKey.GetBytes(24);

                        break;

                    case EncryptionType.Enc256:
                        decAlg.KeySize = 256;
                        decAlg.Key = PrivateKey.GetBytes(32);

                        break;

                }
                decAlg.IV = IVValue;

                MemoryStream decryptionStreamBacking = new MemoryStream();
                CryptoStream decrypt = new CryptoStream(decryptionStreamBacking, decAlg.CreateDecryptor(), CryptoStreamMode.Write);
                decrypt.Write(EncryptedData, 0, EncryptedData.Length);
                decrypt.Flush();
                decrypt.Close();
                PrivateKey.Reset();

                // Get the decrypted data back as a string (how it was before it was encrypted)
                string DecreptedData = new UTF8Encoding(false).GetString(decryptionStreamBacking.ToArray());

                return DecreptedData;
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }


        /// <summary>
        /// Convets plain text password to a SHA256 Hash
        /// </summary>
        /// <param name="PlainTextPassword">The plain text password to hash</param>
        /// <returns>SHA256 Hashed String</returns>
        public static string HashStrting(string PlainTextPassword)
        {
            byte[] hashedData;
            
            // convert the password to a hashed byte array
            using (HashAlgorithm algorithm = SHA256.Create())
                hashedData = algorithm.ComputeHash(Encoding.UTF8.GetBytes(PlainTextPassword));

            // convert the byte array to a hashed string
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashedData)
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }



        /*
        /// <summary>
        /// Uses a password to create a private key and then encrypts the passed in data
        /// </summary>
        /// <param name="TextToEncrypt">The string to encrypt</param>
        /// <param name="passPhrase">The password to use to encrypt the data</param>
        /// <param name="encryptionStrength">The private key size</param>
        /// <returns>The Encrypted data or String.Empty if anything went wrong</returns>
        public static string Encrypt(string TextToEncrypt, string passPhrase, EncryptionType encryptionStrength)
        {
            try
            {
                byte[] SaltValue;

                // create some random numbers. This will be used allong with the password string passed in
                // to create a private key
                SaltValue = Encryption.GenerateSaltValue(Encryption.SaltValueSize);

                // Turns the plane text password into a private encryption key
                Rfc2898DeriveBytes PrivateKey = new Rfc2898DeriveBytes(passPhrase, SaltValue, Encryption.InterationCount);


                // Encrypt the data.
                Aes encAlg = Aes.Create();
                // 16 = 128 bit encryption. 24 = 192 bit encryption. 32 = 256 bit encryption.
                // default is 16 to pass in (128 bit encryption). Might need to change key size if want to use any other size of encryption
                //encAlg.Key = PrivateKey.GetBytes(16);
                switch (encryptionStrength)
                {
                    case EncryptionType.Enc128:
                        encAlg.KeySize = 128;
                        encAlg.Key = PrivateKey.GetBytes(16);

                        break;

                    case EncryptionType.Enc192:
                        encAlg.KeySize = 192;
                        encAlg.Key = PrivateKey.GetBytes(24);

                        break;

                    case EncryptionType.Enc256:
                        encAlg.KeySize = 256;
                        encAlg.Key = PrivateKey.GetBytes(32);

                        break;

                }
                MemoryStream encryptionStream = new MemoryStream();
                CryptoStream encrypt = new CryptoStream(encryptionStream, encAlg.CreateEncryptor(), CryptoStreamMode.Write);
                // convert the unencrypted text to a byte array
                byte[] utfD1 = new System.Text.UTF8Encoding(false).GetBytes(TextToEncrypt);

                // write the unencrypted data into the Crypto Stream (this is where it will get encrypted)
                encrypt.Write(utfD1, 0, utfD1.Length);
                encrypt.FlushFinalBlock();
                encrypt.Close();
                // get back the encrypted data as a byte array
                byte[] EncryptedDataInByteArray = encryptionStream.ToArray();
                PrivateKey.Reset();


                // Convert the byte array to a base 64 value and add the salt and iv values into the data.
                //
                // add the randomly generated salt value & IV to the begining of the ecrypted data
                // then convert the hole thing to base64.
                // The salt value & IV are needed when we want to decrypt the data, so we need to store them somewhere.
                // Good place might be the database, but we are going to put it in with the encrypted data.
                // While not a good idea the user still can't decrypt the data without the password
                // and they would need to know we stored the salt value at the begining of the encrypted data.
                return Convert.ToBase64String(SaltValue.Concat(encAlg.IV).Concat(EncryptedDataInByteArray).ToArray());

                
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
        

        /// <summary>
        ///  Using a password phrase decrypt the passed in data back to plain text
        /// </summary>
        /// <param name="TextToDecrypt">The encrypted data</param>
        /// <param name="passPhrase">The password to use to decrypt the data</param>
        /// <param name="encryptionStrength">The private key size</param>
        /// <returns>The Dycrypted data or String.Empty if anything went wrong</returns>
        public static string Decrypt(string TextToDecrypt, string passPhrase, EncryptionType encryptionStrength)
        {
            try
            {
                // convert the passed in data to a byte array.
                // The passed in data has the salt value & IV stored at the begining of it and then the encrypted data after it.
                // we will need to seperate these out.
                // The Iv size was done when data was being encrypted. It is the size of the blocks when they are encrypted
                // It is normaly 16 is basicaly Aes.BlockSize / 8
                int IVSize = 16;
                byte[] SaltIVAndEncryptedData = Convert.FromBase64String(TextToDecrypt);
                // get the salt value
                byte[] SaltValue = SaltIVAndEncryptedData.Take(Encryption.SaltValueSize).ToArray();
                // get the IV Value (its size is Aes.BlockSize / 8) Default value is 16 (its the size the blocks have been encrypted in)
                byte[] IVValue = SaltIVAndEncryptedData.Skip(Encryption.SaltValueSize).Take(IVSize).ToArray();
                // get the encrypted data
                byte[] EncryptedData = SaltIVAndEncryptedData.Skip(SaltValueSize + IVSize).Take(SaltIVAndEncryptedData.Length - (SaltValueSize + IVSize)).ToArray();

                // turn the plain text password into a private key
                Rfc2898DeriveBytes PrivateKey = new Rfc2898DeriveBytes(passPhrase, SaltValue);

                Aes decAlg = Aes.Create();
                //decAlg.Key = PrivateKey.GetBytes(16);
                // the the key size we will be  using
                switch (encryptionStrength)
                {
                    case EncryptionType.Enc128:
                        decAlg.KeySize = 128;
                        decAlg.Key = PrivateKey.GetBytes(16);

                        break;

                    case EncryptionType.Enc192:
                        decAlg.KeySize = 192;
                        decAlg.Key = PrivateKey.GetBytes(24);

                        break;

                    case EncryptionType.Enc256:
                        decAlg.KeySize = 256;
                        decAlg.Key = PrivateKey.GetBytes(32);

                        break;

                }
                decAlg.IV = IVValue;

                MemoryStream decryptionStreamBacking = new MemoryStream();
                CryptoStream decrypt = new CryptoStream(decryptionStreamBacking, decAlg.CreateDecryptor(), CryptoStreamMode.Write);
                decrypt.Write(EncryptedData, 0, EncryptedData.Length);
                decrypt.Flush();
                decrypt.Close();
                PrivateKey.Reset();

                // Get the decrypted data back as a string (how it was before it was encrypted)
                string DecreptedData = new UTF8Encoding(false).GetString(decryptionStreamBacking.ToArray());

                return DecreptedData;
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }
        */


        /// <summary>
        /// Creates random numbers and returns them in a byte array
        /// </summary>
        /// <param name="size">The numbe of random numbers to create</param>
        /// <returns>Random numbers in a byte array</returns>
        public static byte[] GenerateSaltValue(int size)
        {
            byte[] salt = new byte[size];
            using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with a random value.
                rngCsp.GetBytes(salt);
            }

            return salt;
        }
    }


    /*
    public class Encryption
    {

        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 256;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        public static string Encrypt(string plainText, string passPhrase)
        {
            if (plainText == null || passPhrase == null)
                return string.Empty;

            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }

        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }


    }
    */

    /*
    public static class CipherHelper
    {
        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 256;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        public static string Encrypt(string plainText, string passPhrase)
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                var engine = new RijndaelEngine(256);
                var blockCipher = new CbcBlockCipher(engine);
                var cipher = new PaddedBufferedBlockCipher(blockCipher, new Pkcs7Padding());
                var keyParam = new KeyParameter(keyBytes);
                var keyParamWithIV = new ParametersWithIV(keyParam, ivStringBytes, 0, 32);

                cipher.Init(true, keyParamWithIV);
                var comparisonBytes = new byte[cipher.GetOutputSize(plainTextBytes.Length)];
                var length = cipher.ProcessBytes(plainTextBytes, comparisonBytes, 0);

                cipher.DoFinal(comparisonBytes, length);
                //                return Convert.ToBase64String(comparisonBytes);
                return Convert.ToBase64String(saltStringBytes.Concat(ivStringBytes).Concat(comparisonBytes).ToArray());
            }
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                var engine = new RijndaelEngine(256);
                var blockCipher = new CbcBlockCipher(engine);
                var cipher = new PaddedBufferedBlockCipher(blockCipher, new Pkcs7Padding());
                var keyParam = new KeyParameter(keyBytes);
                var keyParamWithIV = new ParametersWithIV(keyParam, ivStringBytes, 0, 32);

                cipher.Init(false, keyParamWithIV);
                var comparisonBytes = new byte[cipher.GetOutputSize(cipherTextBytes.Length)];
                var length = cipher.ProcessBytes(cipherTextBytes, comparisonBytes, 0);

                cipher.DoFinal(comparisonBytes, length);
                //return Convert.ToBase64String(saltStringBytes.Concat(ivStringBytes).Concat(comparisonBytes).ToArray());

                var nullIndex = comparisonBytes.Length - 1;
                while (comparisonBytes[nullIndex] == (byte)0)
                    nullIndex--;
                comparisonBytes = comparisonBytes.Take(nullIndex + 1).ToArray();


                var result = Encoding.UTF8.GetString(comparisonBytes, 0, comparisonBytes.Length);

                return result;
            }
        }

        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
    */

}
