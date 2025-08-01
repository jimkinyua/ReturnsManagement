using Returns.Helpers.Enums;
using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class WorkflowInstance:CommonFields
    {
        public string WorkflowTemplateId { get; set; } = null!;
        public string? ReturnSubmissionId { get; set; }  // Nullable for Q groups; links to individual submission for non-Q
        public string PeriodId { get; set; } = null!;  // Required for Q groups; nullable for non-Q
        public string SaccoId { get; set; } = null!;
        public string TeamId { get; set; } = null!;
        public string UserId { get; set; } = null!;  // Current assignee
        public string RoleName { get; set; } = null!;
        public string Status { get; set; } = ApprovalStatus.Pending.ToString();
        public int? Rating { get; set; }
        public string CurrentStepId { get; set; } = null!;
        public bool EnforcementTriggered { get; set; } = false;
        public string Type { get; set; } = "Standalone";  // "QGroup" or "Standalone" to differentiate
        public bool CanBeSeen { get; set; } = true;  // Flag to determine if current user can see (updated based on status/assignment)
        [ForeignKey("CurrentStepId")]
        public virtual WorkFlowStep CurrentStep { get; set; } = null!;
        public bool IsComplete { get; internal set; } = false;
        public string? IncompletenessNotes { get; internal set; }
    }
}
