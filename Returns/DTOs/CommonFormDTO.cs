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

        // Individual form versioning properties
        public int CurrentVersion { get; set; } = 1;
        public List<FormVersionDTO> AvailableVersions { get; set; } = new List<FormVersionDTO>();
        public bool HasMultipleVersions { get; set; } = false;
        public string? ExpectedReturnId { get; set; }
        public string? FormCode { get; set; }

        public CommonFormDTO()
        {
            // Set CanAutoAmend based on current date
            var currentDate = DateTime.UtcNow; // Use UTC to align with server time, adjust to EAT if needed
            CanAutoAmend = currentDate.Day < 15;
        }
    }

    public class FormVersionDTO
    {
        public int Version { get; set; }
        public string SubmissionId { get; set; } = null!;
        public DateTime SubmittedAt { get; set; }
        public string Status { get; set; } = null!;
        public bool IsLatest { get; set; }
        public string? AmendsSubmissionId { get; set; }
        public string? AmendedBySubmissionId { get; set; }
        public string? ExpectedReturnId { get; set; }
        public string? FormCode { get; set; }
    }
}
