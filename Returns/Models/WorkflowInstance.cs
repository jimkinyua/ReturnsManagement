using Returns.Helpers.Enums;
using Returns.Models.Common;

namespace Returns.Models
{
    public class WorkflowInstance:CommonFields
    {
     
        public string CurrentStepId { get; set; } = null!;
        public bool IsPublished { get; set; } = false;
        public bool EnforcementTriggered { get; set; } = false;
        public string WorkflowTemplateId { get; set; } = null!;
        public string ReturnId { get; set; } = null!;
        public string Status { get; set; }= ApprovalStatus.Pending.ToString();
        public int? Rating { get; set; }  // CAMELS rating (1-5, nullable if not set yet)

    }
}
