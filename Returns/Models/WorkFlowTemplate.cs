using Returns.Models.Common;

namespace Returns.Models
{
    public class WorkFlowTemplate:CommonFields
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsPublished { get; set; } = false;
        public virtual ICollection<WorkFlowStep> WorkFlowSteps { get; set; } = new HashSet<WorkFlowStep>();

    }
}
