using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class ApprovalAction:CommonFields
    {
  
        public string UserId { get; set; } = null!;
        [ForeignKey("WorkFlowStep")]
        public string WorkFlowStepId { get; set; } = null!;
        public string ReturnId { get; set; } = null!;
        public string Comment { get; set; } = null!;
        public string Status { get; set; } = null!;
        public virtual WorkFlowStep WorkFlowStep { get; set; } = null!;
    }
}
