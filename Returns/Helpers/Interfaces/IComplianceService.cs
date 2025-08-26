using Returns.DTOs.Compliance;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Returns.Helpers.Interfaces
{

    public class UserDTO
    {
        [JsonConverter(typeof(NumberToStringConverter))]
        public string UserId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public bool IsTeamLead { get; set; }
        [JsonConverter(typeof(NumberToStringConverter))]
        public string? TeamId { get; set; }
    }

    public class NumberToStringConverter : JsonConverter<string>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                // Handle integer (adjust to GetInt64/GetDouble if larger numbers expected)
                return reader.GetInt32().ToString();
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString();
            }
            else if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            throw new JsonException($"Unexpected token type '{reader.TokenType}' for string property.");
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }

    public class SaccoAssignmentDTO
    {
        [JsonConverter(typeof(NumberToStringConverter))]
        public string SaccoId { get; set; } = null!;
        public string SaccoName { get; set; } = null!;
        public UserDetailsDTO? AssignedUser { get; set; } 
        public DateTime? AssignedAt { get; set; }
    }

    public class TeamLeadDTO : UserDTO { }  // Inherits from UserDTO

    public class TeamMemberDTO : UserDTO { }  // Inherits from UserDTO

    public class SaccoDTO
    {
        [JsonConverter(typeof(NumberToStringConverter))]
        public string SaccoId { get; set; }
        public string SaccoName { get; set; } = string.Empty;
        [JsonConverter(typeof(NumberToStringConverter))]
        public string SaccoType { get; set; } = string.Empty;
        public string OfficialEmail { get; set; } = string.Empty;
        public string CooperativeSocietyNo { get; set; } = string.Empty;
    }



    public class RoleDTO
    {
        [JsonConverter(typeof(NumberToStringConverter))]
        public string RoleId { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public string Description { get; set; } = null!;
    }


    public interface IComplianceService
    {
        Task<ComplianceOfficerInfo> GetAssignedComplianceOfficer(string saccoId);
        Task<SasraUser?> GetTeamLead(string teamId);
        Task<List<SasraUser>?> GetTeamMembers(string teamId);
        //Task<SasraUser?> GetUsersByRole(string RoleId);
        Task<List<MinSasraUser>> GetUsersByRole(string RoleId);
        Task<SasraUser?> GetUserById(string UserId);
        Task<NormalisedRoleName?> GetRoleDetails(string RoleId);
        Task<List<Sacco>> GetAllSaccosAsync();
        Task<SaccoDTO> GetSaccoByIdAsync(long saccoId);
        Task<List<Sacco>> GetSaccosAssignedToOfficerAsync(string userId);
        Task<string?> GetTeamIdForSaccoAsync(string saccoId);
        Task <UserDTO>GetUserDetailsAsync(string tlUserId);
        Task<List<SaccoDTO>> GetSaccosForTeamAsync(string teamId);  
        Task<List<TeamMemberDTO>> GetTeamMembersAsync(string teamId);  
        Task<string?> GetTeamIdForUserAsync(string userId);
        Task<TeamLeadDTO> GetTeamLeaderAsync(string teamId);
        Task<List<SaccoDTO>> GetSaccosForTheTeamAsync(string teamId);
        Task<SaccoDTO> GetSaccoByTheirIdAsync(string saccoId);



    }
}
