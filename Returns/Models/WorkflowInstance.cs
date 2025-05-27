using Returns.Helpers.Enums;
using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class WorkflowInstance:CommonFields
    {
     
        public string RoleName { get; set; } = null!;
        public bool EnforcementTriggered { get; set; } = false;
        public string WorkflowTemplateId { get; set; } = null!;
        [ForeignKey("Returns")]
        public string ReturnId { get; set; } = null!;
        public string TeamId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string Status { get; set; }= ApprovalStatus.Pending.ToString();
        public int? Rating { get; set; }
        [ForeignKey("WorkFlowStep")]
        public string CurrentStepId { get; set; } = null!;
        public string SaccoId { get; set; } = null!;
        public virtual WorkFlowStep CurrentStep { get; set; } = null!;
        public virtual Return Return { get; set; } = null!;

    }
}
