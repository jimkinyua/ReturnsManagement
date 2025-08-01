using static Returns.Helpers.ReturnsHelper;

namespace Returns.DTOs
{
    public class CommonFormDTO
    {
        public string? FormId { get; set; } = null!;
        public string FileUrl { get; set; } = string.Empty;
        public string SubmissionId { get; set; } = string.Empty;
        public bool CanAutoAmend { get; private set; } = false; 
        public bool RequiresResubmission { get; set; } = false;
        public List<VersionChoice> PreviousVersionIds { get; set; } = new List<VersionChoice>();
        public CommonFormDTO()
        {
            // Set CanAutoAmend based on current date
            var currentDate = DateTime.UtcNow; // Use UTC to align with server time, adjust to EAT if needed
            CanAutoAmend = currentDate.Day < 15;
        }
    }
}
