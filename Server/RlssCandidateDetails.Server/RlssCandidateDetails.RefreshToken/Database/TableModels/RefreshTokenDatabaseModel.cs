namespace RlssCandidateDetails.RefreshToken.Database.TableModels
{
    public class RefreshTokenDatabaseModel
    {
        public string TokenID { get; set; } = string.Empty;
        public int CurrentVersionNumber { get; set; } = -1;

        /// <summary>
        /// The date/time the refresh token expiers (this is in Universal Time)
        /// </summary>
        public DateTime UtcExpiryDate { get; set; }

        /// <summary>
        /// The ID of the customer this Refresh Token belongs too
        /// </summary>
        public int CustomerID { get; set; } = -1;


        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        public static int ConvertDateTimeToUnixTimeStamp(DateTime datetime)
        {
            Int32 unixTimestamp = (Int32)(datetime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            return unixTimestamp;
        }
    }
}
