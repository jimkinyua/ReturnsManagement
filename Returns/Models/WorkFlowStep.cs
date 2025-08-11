using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public enum StepAssignee
    {
        Officer = 0,
        TeamLead = 1,
        SpecificUser = 2
    }

    public class WorkFlowStep:CommonFields
    {
        public int Sequence { get; set; }
        public string RoleId { get; set; } = null!;
        public string? RoleName { get; set; }
        public string? SpecificUserId { get; set; }
        public StepAssignee AssigneeType { get; set; }

        [ForeignKey("WorkFlowTemplate")]
        public string WorkFlowTemplateId { get; set; } = null!;
        public virtual WorkFlowTemplate WorkFlowTemplate { get; set; } = null!;
        public virtual ICollection<ApprovalAction> ApprovalActions { get; set; } = new HashSet<ApprovalAction>();
        public virtual ICollection<WorkflowInstance> WorkflowInstances { get; set; } = new HashSet<WorkflowInstance>();
    }
}
