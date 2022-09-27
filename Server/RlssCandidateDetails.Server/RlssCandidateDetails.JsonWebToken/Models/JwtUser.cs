namespace RlssCandidateDetails.JsonWebToken.Models
{
    public class JwtUser
    {
        public int UserID { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }

        public List<JwtRole> RolesUserHas { get; set; } = new List<JwtRole>();
    }
}
