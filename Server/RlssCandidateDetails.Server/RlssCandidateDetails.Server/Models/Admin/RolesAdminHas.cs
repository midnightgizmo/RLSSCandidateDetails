namespace RlssCandidateDetails.Server.Models.Admin
{
    public class RolesAdminHas
    {
        public int AdminLoginCredentialsId { get; set; } = 0;
        public int RolesId { get; set; } = 0;

        public string RoleName { get; set; } = string.Empty;
    }
}
