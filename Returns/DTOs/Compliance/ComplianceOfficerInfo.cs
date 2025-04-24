namespace Returns.DTOs.Compliance
{
    public class ComplianceOfficerInfo
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public string TeamRole { get; set; } = string.Empty;
        public string TeamId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class TeamLead
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public string TeamRole { get; set; } = string.Empty;
        public string TeamId { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;

    }


    public class SasraUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public string TeamRole { get; set; } = string.Empty;
        public string TeamId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;

    }


}
