namespace RlssCandidateDetails.Server.Models.Candidate
{
    public class CandidateDetails
    {
        public int ID { get; set; } = 0;
        public string FirstName { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string SocietyNumber { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; } = null;

        /// <summary>
        /// Check each char in the society number to see if its a number
        /// </summary>
        /// <param name="SocietyNumber">value to check to see if its a number</param>
        /// <returns>true if SocietyNumber is empty or a number. false if not a number </returns>
        public static bool IsValidSocietyNumber(string SocietyNumber)
        {
            // check each char in the string to make sure it is a number
            foreach (char num in SocietyNumber)
            {
                int number;
                if (int.TryParse(num.ToString(), out number) == false)
                    return false;
            }

            return true;


        }
    }
}
