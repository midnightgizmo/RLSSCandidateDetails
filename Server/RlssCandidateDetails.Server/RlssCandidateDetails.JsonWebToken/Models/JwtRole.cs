namespace RlssCandidateDetails.JsonWebToken.Models
{
    /// <summary>
    /// A single role which can be assinged to a <see cref="JwtUser"/>
    /// </summary>
    public class JwtRole
    {
        /// <summary>
        /// The ID the role has been given in the database
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// The name of the Role
        /// </summary>
        public string Name { get; set; }
    }
}
