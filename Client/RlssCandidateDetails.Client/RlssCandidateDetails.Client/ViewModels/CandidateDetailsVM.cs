using System.ComponentModel.DataAnnotations;

namespace RlssCandidateDetails.Client.ViewModels
{
    public class CandidateDetailsVM
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(10, ErrorMessage = "Name is too long")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Surname is required")]
        [StringLength(10, ErrorMessage = "Surname is too long")]
        public string Surname { get; set; } = string.Empty;

        [RegularExpression(@"^\d{8}$", ErrorMessage = "Must be 8 digits long")]
        public int? SocietyNumber { get; set; } = null;
    }
}
