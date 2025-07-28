using Returns.Models.Common;

namespace Returns.Models
{
    public class SaccoAssignment : CommonFields
    {
        public string SaccoId { get; set; } = null!; 
        public string AssignedUserId { get; set; } = null!; 
        public string AssignedByUserId { get; set; } = null!;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
