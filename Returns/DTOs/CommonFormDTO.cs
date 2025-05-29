using static Returns.Helpers.ReturnsHelper;

namespace Returns.DTOs
{
    public class CommonFormDTO
    {
        public string? FormId { get; set; } = null!;
        public Boolean RequiresResubmission { get; set; } = false;
        public List<VersionChoice> PreviousVersionIds { get; set; } = new List<VersionChoice>();

    }
}
