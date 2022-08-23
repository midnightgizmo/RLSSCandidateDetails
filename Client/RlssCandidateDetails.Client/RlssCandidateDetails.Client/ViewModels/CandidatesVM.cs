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
        //[RegularExpression(@"^\d{8}$", ErrorMessage ="Must be 8 digits long")]
		[RegularExpression(@"^[0-9]{1,8}$", ErrorMessage ="Must be a number of no more than 8 digits long")]
        public int? SocietyNumber { get; set; } = null;

        public List<CandidatesVM_CandidateDeetails> CandidatesToShow { get; set; }

        public CandidatesVM()
        {
            this.CandidatesToShow = new List<CandidatesVM_CandidateDeetails>();
        }
    }

    public class CandidatesVM_CandidateDeetails : Candidate
    {
        /// <summary>
        /// Keeps track of when the user clicks the delete button on a candidate row.
        /// </summary>
        public bool IsDeleteButtonClicked { get; set; } = false;
    }
}
