using System.ComponentModel.DataAnnotations;

namespace RlssCandidateDetails.Client.ViewModels
{
    public class LoginVM
    {
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
