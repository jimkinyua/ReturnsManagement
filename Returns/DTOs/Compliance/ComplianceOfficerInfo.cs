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
        public string RoleId { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;

    }

    public class MinSasraUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
    }


    public class SasraRoleDetails
    {
        public string RoleName { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;
    }

    public class Sacco
    {
        public string Id { get; set; }
        public string SaccoName { get; set; }
        public string OfficialSaccoEmail { get; set; }
        public string ContactNumber { get; set; }
        public string KraPin { get; set; }
        public string SaccoType { get; set; }
        public bool IsApproved { get; set; }
        public string AuthorizedRepresentative { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string CooperativeSocietyNo { get; set; }
        public string TeamId { get; set; }
        public string TeamName { get; set; }
    }

}
