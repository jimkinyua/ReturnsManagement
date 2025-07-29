namespace Returns.Helpers.Interfaces
{
    public class UserDetailsDTO
    {
        public string UserId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsTeamLead { get; set; }
        public string? TeamId { get; set; }
    }

  

    public interface ISaccoAssignmentService
    {
        Task AssignSaccoToMemberAsync(string saccoId, string memberId, string teamLeadId);
        Task<List<SaccoDTO>> GetAssignableSaccosAsync(string teamLeadId);
        Task<List<TeamMemberDTO>> GetTeamMembersAsync(string teamLeadId);
        Task UnassignSaccoAsync(string saccoId, string teamLeadId);
        Task<List<SaccoAssignmentDTO>> GetTeamAssignmentsAsync(string tlUserId);
    }
}
