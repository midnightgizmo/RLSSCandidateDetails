using RlssCandidateDetails.Client.Models;
using System.ComponentModel.DataAnnotations;

namespace RlssCandidateDetails.Client.ViewModels
{
    public class CandidatesVM
    {
        [StringLength(10, ErrorMessage="Name is too long")]
        public string FirstNameFilter {get;set;}
        [StringLength(10, ErrorMessage = "Surname is too long")]
        public string SurnameFilter {get;set;}
        [RegularExpression(@"^\d{8}$", ErrorMessage ="Must be 8 digits long")]
        public int? SocietyNumber { get; set; } = null;

        public List<Candidate> CandidatesToShow { get; set; }

        public CandidatesVM()
        {
            this.CandidatesToShow = new List<Candidate>();
        }
    }
}
