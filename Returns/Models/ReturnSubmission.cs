using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class ReturnSubmission:CommonFields
    {
        [ForeignKey("ExpectedReturn")]
        public string ExpectedReturnId { get; set; } = null!;  
        public ExpectedReturn ExpectedReturn { get; set; } = null!;
        public string SaccoId { get; set; } = null!;
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
        public string FileUrl { get; set; } = null!;  
        public bool IsActive { get; set; } = true;
        public ICollection<DTCapitalAdequacyReturn> CapitalAdequacies { get; set; }= new List<DTCapitalAdequacyReturn>();
    }
}
