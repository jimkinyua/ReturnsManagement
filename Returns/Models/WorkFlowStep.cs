using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class WorkFlowStep:CommonFields
    {
        public int Sequence { get; set; }
        public string RoleId { get; set; } = null!;
        public string RoleName { get; set; } = null!;

        [ForeignKey("WorkFlowTemplate")]
        public string WorkFlowTemplateId { get; set; } = null!;
        public virtual WorkFlowTemplate WorkFlowTemplate { get; set; } = null!;
        public virtual ICollection<ApprovalAction> ApprovalActions { get; set; } = new HashSet<ApprovalAction>();
    }
}
