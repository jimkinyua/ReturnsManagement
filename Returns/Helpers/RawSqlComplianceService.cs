using Microsoft.Data.SqlClient;
using Returns.DTOs.Compliance;
using Returns.Helpers.Interfaces;
using System.Data;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Returns.Helpers
{
    public class RawSqlComplianceService : IComplianceService
    {

        private readonly string _connectionString;
        private readonly ILogger<RawSqlComplianceService> _logger;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };


        public RawSqlComplianceService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<RawSqlComplianceService> logger)
        {
            _httpClient = httpClient;
            _connectionString = configuration.GetConnectionString("IdentityDbConnection")!;
            _logger = logger;
        }

        public async Task<ComplianceOfficerInfo> GetAssignedComplianceOfficer(string saccoId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string sql = @"
                        SELECT TOP (1)
                        u.Id,
                        u.FullName,
                        u.Email,
	                    T.Name as TeamName,
                        u.TeamRole,
                        r.Name  AS RoleName,
                        u.TeamId
                    FROM UserSaccos        us
                    JOIN AspNetUsers       u  ON u.Id = CAST(us.UserId AS nvarchar(450))
                    JOIN AspNetUserRoles   ur ON ur.UserId = u.Id
                    JOIN AspNetRoles       r  ON r.Id = ur.RoleId
                    JOIN Teams T On T.Id = u.TeamId
                    WHERE us.SaccoId = @saccoId";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add(new SqlParameter("@saccoId", SqlDbType.NVarChar) { Value = saccoId });
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var officer = new ComplianceOfficerInfo
                                {
                                    Id = reader["Id"]?.ToString() ?? string.Empty,
                                    FullName = reader["FullName"]?.ToString() ?? string.Empty,
                                    Email = reader["Email"]?.ToString() ?? string.Empty,
                                    TeamName = reader["TeamName"]?.ToString() ?? string.Empty,
                                    TeamRole = reader["TeamRole"]?.ToString() ?? string.Empty,
                                    TeamId = reader["TeamId"]?.ToString() ?? string.Empty,
                                    Role = reader["RoleName"]?.ToString() ?? string.Empty

                                };
                                return officer;
                            }
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving compliance officer for SACCO ID {SaccoId}", saccoId);
                return null;
            }
        }


        public async Task<List<Sacco>> GetAllSaccosAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string sql = @"
                        SELECT  
                        u.[Id],
                        u.[SaccoName],
                        u.[OfficialSaccoEmail],
                        u.[ContactNumber],
                        u.[Kra_Pin],
                        u.[SaccoType],
                        u.[IsApproved],
                        u.[AuthorizedRepresentative],
                        u.[ApprovedAt],
                        u.[CooperativeSocietyNo],
                        u.[TeamId]
                    FROM [Saccos] u
                    ";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var saccos = new List<Sacco>();
                            while (await reader.ReadAsync())
                            {
                                var sacco = new Sacco
                                {
                                    Id = reader["Id"]?.ToString() ?? string.Empty,
                                    SaccoName = reader["SaccoName"]?.ToString() ?? string.Empty,
                                    OfficialSaccoEmail = reader["OfficialSaccoEmail"]?.ToString() ?? string.Empty,
                                    ContactNumber = reader["ContactNumber"]?.ToString() ?? string.Empty,
                                    KraPin = reader["Kra_Pin"]?.ToString() ?? string.Empty,
                                    SaccoType = reader["SaccoType"]?.ToString() ?? string.Empty,
                                    IsApproved = reader["IsApproved"] as bool? ?? false,
                                    AuthorizedRepresentative = reader["AuthorizedRepresentative"]?.ToString() ?? string.Empty,
                                    ApprovedAt = reader["ApprovedAt"] as DateTime?,
                                    CooperativeSocietyNo = reader["CooperativeSocietyNo"]?.ToString() ?? string.Empty,
                                    TeamId = string.Empty, //reader["TeamId"]?.ToString() ?? string.Empty,
                                    TeamName = string.Empty, //reader["TeamName"]?.ToString() ?? string.Empty
                                };
                                saccos.Add(sacco);
                            }
                            return saccos;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all Saccos");
                return new List<Sacco>();
            }
        }

        public async Task<Sacco> GetSaccoByIdAsync(string saccoId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string sql = @"
                SELECT 
                    [Id],
                    [SaccoName],
                    [OfficialSaccoEmail],
                    [ContactNumber],
                    [Kra_Pin],
                    [SaccoType],
                    [IsApproved],
                    [AuthorizedRepresentative],
                    [ApprovedAt],
                    [CooperativeSocietyNo],
                    [TeamId],
                    [TeamName]
                FROM [Saccos]
                WHERE Id = @saccoId";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add(new SqlParameter("@saccoId", SqlDbType.NVarChar) { Value = saccoId });

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new Sacco
                                {
                                    Id = reader["Id"]?.ToString() ?? string.Empty,
                                    SaccoName = reader["SaccoName"]?.ToString() ?? string.Empty,
                                    OfficialSaccoEmail = reader["OfficialSaccoEmail"]?.ToString() ?? string.Empty,
                                    ContactNumber = reader["ContactNumber"]?.ToString() ?? string.Empty,
                                    KraPin = reader["Kra_Pin"]?.ToString() ?? string.Empty,
                                    SaccoType = reader["SaccoType"]?.ToString() ?? string.Empty,
                                    IsApproved = reader["IsApproved"] as bool? ?? false,
                                    AuthorizedRepresentative = reader["AuthorizedRepresentative"]?.ToString() ?? string.Empty,
                                    ApprovedAt = reader["ApprovedAt"] as DateTime?,
                                    CooperativeSocietyNo = reader["CooperativeSocietyNo"]?.ToString() ?? string.Empty,
                                    TeamId = reader["TeamId"]?.ToString() ?? string.Empty,
                                    TeamName = reader["TeamName"]?.ToString() ?? string.Empty
                                };
                            }
                            return null; // Return null if no sacco found
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Sacco with ID {SaccoId}", saccoId);
                throw; // Re-throw the exception to let the caller handle it
            }
        }
        public Task<SasraUser?> GetTeamLead(string teamId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = @"  
                           SELECT TOP (1)
                             u.Id,
                             u.FullName,
                             u.Email,
                             T.Name as TeamName,
                             u.TeamRole
                         FROM Teams AS T
                         INNER JOIN AspNetUsers u ON u.TeamId = T.Id
                         INNER JOIN AspNetRoles AS r ON r.Id = CONVERT(NVARCHAR(450), u.Role)
                         WHERE 
                             T.Id    = @teamId AND u.TeamRole = 1;";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add(new SqlParameter("@teamId", SqlDbType.NVarChar) { Value = teamId });
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var officer = new SasraUser
                                {
                                    Id = reader["Id"]?.ToString()??string.Empty,
                                    FullName = reader["FullName"]?.ToString() ?? string.Empty,
                                    Email = reader["Email"]?.ToString() ?? string.Empty,
                                    TeamName = reader["TeamName"]?.ToString() ?? string.Empty,
                                    TeamRole = reader["TeamRole"]?.ToString() ?? string.Empty,
                                };
                                return Task.FromResult<SasraUser?>(officer);
                            }
                            return Task.FromResult<SasraUser?>(null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving team lead for Team ID {TeamId}", teamId);
                return Task.FromResult<SasraUser?>(null);
            }
        }

        public Task<SasraUser?> GetUserById(string UserId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = @"  
                        	 SELECT TOP 1 u.Id
                            ,[FullName]
                            ,[LastName]
                            ,[FirstName]
                            ,[Email]
                            ,T.Name as TeamName
                            ,[RoleId]
                            ,[TeamRole]
                              FROM [AspNetUsers] as u
                              JOIN AspNetUserRoles   ur ON ur.UserId = u.Id
                              JOIN AspNetRoles   r  ON r.Id = ur.RoleId
                              Join Teams T On T.Id = u.TeamId
                              WHERE u.Id = @UserId";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var officer = new SasraUser
                                {
                                    Id = reader["Id"]?.ToString() ?? string.Empty,
                                    FullName = reader["FullName"]?.ToString() ?? string.Empty,
                                    Email = reader["Email"]?.ToString() ?? string.Empty,
                                    RoleId = reader["RoleId"]?.ToString() ?? string.Empty,
                                    TeamName= reader["TeamName"]?.ToString() ?? string.Empty,
                                    TeamRole = reader["TeamRole"]?.ToString() ?? string.Empty,
                                };
                                return Task.FromResult<SasraUser?>(officer);
                            }
                            return Task.FromResult<SasraUser?>(null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving team lead for User ID {TeamId}", UserId);
                return Task.FromResult<SasraUser?>(null);
            }
        }


        public Task<SasraUser?> GetUserByRole(string RoleId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = @"  
                       SELECT 
                           U.FullName,
                           U.Id,
                           u.Email,
                           r.Id AS RoleId,
                           R.Name AS RoleName
                        FROM AspNetUsers U
                        INNER JOIN AspNetRoles R
                           ON R.Id = CAST(U.Role AS NVARCHAR(450))
                          AND R.Id = @RoleId ";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add(new SqlParameter("@RoleId", SqlDbType.NVarChar) { Value = RoleId });
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var officer = new SasraUser
                                {
                                    Id = reader["Id"]?.ToString() ?? string.Empty,
                                    FullName = reader["FullName"]?.ToString() ?? string.Empty,
                                    Email = reader["Email"]?.ToString() ?? string.Empty,
                                    RoleId = reader["RoleId"]?.ToString() ?? string.Empty,
                                };
                                return Task.FromResult<SasraUser?>(officer);
                            }
                            return Task.FromResult<SasraUser?>(null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving team lead for Team ID {TeamId}", RoleId);
                return Task.FromResult<SasraUser?>(null);
            }
        }

        public async Task<List<Sacco>> GetSaccosAssignedToOfficerAsync(string userId)
        {
            var saccos = new List<Sacco>();

            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                      SELECT  s.Id,
                        s.SaccoName,
                        s.OfficialSaccoEmail,
                        s.ContactNumber,
                        s.Kra_Pin,
                        s.SaccoType,
                        s.IsApproved,
                        s.AuthorizedRepresentative,
                        s.ApprovedAt,
                        s.CooperativeSocietyNo,
                        t.Id AS TeamId,
                        t.Name AS TeamName
                FROM    UserSaccos us
                JOIN    [Saccos] s ON s.Id = us.SaccoId
                Join AspNetUsers as U on U.Id = us.UserId
                Join Teams as T ON T.Id = U.TeamId
                WHERE   us.UserId = @userId;";

                await using var cmd = new SqlCommand(sql, connection);
                cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.NVarChar) { Value = userId });

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    saccos.Add(new Sacco
                    {
                        Id = reader["Id"]?.ToString() ?? string.Empty,
                        SaccoName = reader["SaccoName"]?.ToString() ?? string.Empty,
                        OfficialSaccoEmail = reader["OfficialSaccoEmail"]?.ToString() ?? string.Empty,
                        ContactNumber = reader["ContactNumber"]?.ToString() ?? string.Empty,
                        KraPin = reader["Kra_Pin"]?.ToString() ?? string.Empty,
                        SaccoType = reader["SaccoType"]?.ToString() ?? string.Empty,
                        IsApproved = reader["IsApproved"] as bool? ?? false,
                        AuthorizedRepresentative = reader["AuthorizedRepresentative"]?.ToString() ?? string.Empty,
                        ApprovedAt = reader["ApprovedAt"] as DateTime?,
                        CooperativeSocietyNo = reader["CooperativeSocietyNo"]?.ToString() ?? string.Empty,
                        TeamId = reader["TeamId"]?.ToString() ?? string.Empty,
                        TeamName = reader["TeamName"]?.ToString() ?? string.Empty
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Saccos assigned to user {UserId}", userId);
            }

            return saccos;
        }

        public async Task<List<SasraUser>> GetTeamMembers(string teamId)
        {
            var members = new List<SasraUser>();

            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                    SELECT  u.Id,
                            u.FullName,
                            u.Email,
                            T.Name as TeamName,
                            u.TeamRole,                       -- INT in table
                            r.Id   AS RoleId,
                            r.Name AS RoleName
                    FROM    AspNetUsers       u
                        JOIN AspNetRoles  r ON r.Id = CONVERT(NVARCHAR(450), u.Role)
	                    Join Teams T ON T.Id = u.TeamId
                    WHERE   u.TeamId = @teamId;";

                await using var cmd = new SqlCommand(sql, connection);
                cmd.Parameters.Add(new SqlParameter("@teamId", SqlDbType.NVarChar) { Value = teamId });

                await using var reader = await cmd.ExecuteReaderAsync();

                // Ordinals once for speed / safety
                int ordId = reader.GetOrdinal("Id");
                int ordFullName = reader.GetOrdinal("FullName");
                int ordEmail = reader.GetOrdinal("Email");
                int ordTeamName = reader.GetOrdinal("TeamName");
                int ordTeamRole = reader.GetOrdinal("TeamRole");   // INT
                int ordRoleId = reader.GetOrdinal("RoleId");
                int ordRoleName = reader.GetOrdinal("RoleName");

                while (await reader.ReadAsync())
                {
                    members.Add(new SasraUser
                    {
                        Id = !reader.IsDBNull(ordId) ? reader.GetString(ordId) : string.Empty,
                        FullName = !reader.IsDBNull(ordFullName) ? reader.GetString(ordFullName) : string.Empty,
                        Email = !reader.IsDBNull(ordEmail) ? reader.GetString(ordEmail) : string.Empty,
                        TeamName = !reader.IsDBNull(ordTeamName) ? reader.GetString(ordTeamName) : string.Empty,
                        TeamRole = !reader.IsDBNull(ordTeamRole) ? reader.GetInt32(ordTeamRole).ToString() : string.Empty,
                        RoleId = !reader.IsDBNull(ordRoleId) ? reader.GetString(ordRoleId) : string.Empty,
                        //RoleName = !reader.IsDBNull(ordRoleName) ? reader.GetString(ordRoleName) : string.Empty
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving members for Team ID {TeamId}", teamId);
            }

            return members;
        }


        public async Task<SasraRoleDetails?> GetRoleDetails(string RoleId)
        {
            try
            {
                var resp = await _httpClient.GetAsync($"/api/auth/roles/{RoleId}");
                if (!resp.IsSuccessStatusCode)
                {
                    var errorContent = await resp.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Failed to get role details for {RoleId}. Status: {resp.StatusCode}. Error: {errorContent}");
                }

                await using var stream = await resp.Content.ReadAsStreamAsync();
                var roleDetails = await JsonSerializer.DeserializeAsync<SasraRoleDetails>(stream, _jsonOpts);

                return roleDetails ?? throw new InvalidDataException($"Deserialized role details for {RoleId} is null.");
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException($"JSON deserialization error for role {RoleId}: {ex.Message}", ex);
            }
            catch (HttpRequestException ex)
            {
                // Already thrown with details, or rethrow/log
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error getting role details for {RoleId}: {ex.Message}", ex);
            }
        }

        public async Task<string?> GetTeamIdForSaccoAsync(string saccoId)
        {
            try
            {
                var resp = await _httpClient.GetAsync($"/api/auth/saccos/{saccoId}/team-id");
                if (!resp.IsSuccessStatusCode)
                {
                    var errorContent = await resp.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Failed to get team ID for sacco {saccoId}. Status: {resp.StatusCode}. Error: {errorContent}");
                }

                var content = await resp.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(content))
                    return null;

                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                // if the endpoint returns just a raw string:
                if (root.ValueKind == JsonValueKind.String)
                    return root.GetString();

                // or if it returns { "teamId": "..." } or { "teamId": 123 }
                if (root.TryGetProperty("teamId", out var prop))
                {
                    if (prop.ValueKind == JsonValueKind.String)
                        return prop.GetString();
                    else if (prop.ValueKind == JsonValueKind.Number)
                        return prop.GetInt32().ToString();  // or GetInt64() if large numbers possible
                }

                return null;
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException($"JSON parsing error for team ID of sacco {saccoId}: {ex.Message}", ex);
            }
            catch (HttpRequestException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error getting team ID for sacco {saccoId}: {ex.Message}", ex);
            }
        }

        public async Task<UserDTO> GetUserDetailsAsync(string tlUserId)
        {
            try
            {
                var relativeUri = $"/api/auth/users/{tlUserId}/details";
                var fullUri = new Uri(_httpClient.BaseAddress ?? throw new InvalidOperationException("BaseAddress not set"), relativeUri);

                // Log the full URL (use your logging framework, e.g., ILogger)
                _logger.LogInformation("Requesting full URL: {FullUrl}", fullUri.ToString());

                var resp = await _httpClient.GetAsync(relativeUri);
                if (!resp.IsSuccessStatusCode)
                {
                    var errorContent = await resp.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Failed to get user details for {tlUserId}. Status: {resp.StatusCode}. Error: {errorContent}");
                }

                var content = await resp.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(content))
                    return null;

                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                await using var stream = await resp.Content.ReadAsStreamAsync();
                var user = await JsonSerializer.DeserializeAsync<UserDTO>(stream, _jsonOpts);

                return user ?? throw new InvalidDataException($"Deserialized user details for {tlUserId} is null.");
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException($"JSON deserialization error for user {tlUserId}: {ex.Message}", ex);
            }
            catch (HttpRequestException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error getting user details for {tlUserId}: {ex.Message}", ex);
            }
        }

        public async Task<List<SaccoDTO>> GetSaccosForTeamAsync(string teamId)
        {
            try
            {
                // Call the gateway endpoint for this team’s sacco list
                var resp = await _httpClient.GetAsync($"gateway/api/auth/teams/{teamId}/saccos-list");
                if (!resp.IsSuccessStatusCode)
                {
                    var errorContent = await resp.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Failed to get saccos for team {teamId}. Status: {resp.StatusCode}. Error: {errorContent}");
                }

                // Read content as string to inspect/log
                var jsonContent = await resp.Content.ReadAsStringAsync();

                // Log the JSON (assuming you have an ILogger<RawSqlComplianceService> _logger injected)
                _logger.LogInformation("JSON response for saccos of team {TeamId}: {JsonContent}", teamId, jsonContent);

                // For debugging, you could also breakpoint here or Console.WriteLine(jsonContent);

                // Deserialize from the string using MemoryStream
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));
                var saccos = await JsonSerializer.DeserializeAsync<List<SaccoDTO>>(stream, _jsonOpts);

                // Return an empty list if the API returned null
                return saccos ?? new List<SaccoDTO>();
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException($"JSON deserialization error for saccos of team {teamId}: {ex.Message}", ex);
            }
            catch (HttpRequestException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error getting saccos for team {teamId}: {ex.Message}", ex);
            }
        }

        public async Task<List<TeamMemberDTO>> GetTeamMembersAsync(string teamId)
        {
            try
            {
                var resp = await _httpClient.GetAsync($"/api/auth/teams/{teamId}/members");
                if (!resp.IsSuccessStatusCode)
                {
                    var errorContent = await resp.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Failed to get members for team {teamId}. Status: {resp.StatusCode}. Error: {errorContent}");
                }

                await using var stream = await resp.Content.ReadAsStreamAsync();
                var members = await JsonSerializer.DeserializeAsync<List<TeamMemberDTO>>(stream, _jsonOpts);

                return members ?? new List<TeamMemberDTO>();
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException($"JSON deserialization error for members of team {teamId}: {ex.Message}", ex);
            }
            catch (HttpRequestException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error getting members for team {teamId}: {ex.Message}", ex);
            }
        }

        public async Task<string?> GetTeamIdForUserAsync(string userId)
        {
            try
            {
                var resp = await _httpClient.GetAsync($"/api/auth/users/{userId}/team-id");
                if (!resp.IsSuccessStatusCode)
                {
                    var errorContent = await resp.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Failed to get team ID for user {userId}. Status: {resp.StatusCode}. Error: {errorContent}");
                }

                var content = await resp.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(content))
                    return null;

                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.String)
                    return root.GetString();

                if (root.TryGetProperty("teamId", out var t))
                    return t.GetString();

                return null;
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException($"JSON parsing error for team ID of user {userId}: {ex.Message}", ex);
            }
            catch (HttpRequestException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error getting team ID for user {userId}: {ex.Message}", ex);
            }
        }

        public async Task<TeamLeadDTO> GetTeamLeaderAsync(string teamId)
        {
            try
            {
                var resp = await _httpClient.GetAsync($"/api/auth/teams/{teamId}/team-lead");
                if (!resp.IsSuccessStatusCode)
                {
                    var errorContent = await resp.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Failed to get team lead for {teamId}. Status: {resp.StatusCode}. Error: {errorContent}");
                }

                await using var stream = await resp.Content.ReadAsStreamAsync();
                var lead = await JsonSerializer.DeserializeAsync<TeamLeadDTO>(stream, _jsonOpts)
                           ?? throw new InvalidDataException($"Deserialized team lead for {teamId} is null.");

                return lead;
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException($"JSON deserialization error for team lead of {teamId}: {ex.Message}", ex);
            }
            catch (HttpRequestException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error getting team lead for {teamId}: {ex.Message}", ex);
            }
        }

        public async Task<List<SaccoDTO>> GetSaccosForTheTeamAsync(string teamId)
        {
            try
            {
                var resp = await _httpClient.GetAsync($"/api/auth/teams/{teamId}/saccos-list");
                if (!resp.IsSuccessStatusCode)
                {
                    var errorContent = await resp.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Failed to get saccos for team {teamId}. Status: {resp.StatusCode}. Error: {errorContent}");
                }

                await using var stream = await resp.Content.ReadAsStreamAsync();
                var list = await JsonSerializer.DeserializeAsync<List<SaccoDTO>>(stream, _jsonOpts);

                return list ?? new List<SaccoDTO>();
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException($"JSON deserialization error for saccos of team {teamId}: {ex.Message}", ex);
            }
            catch (HttpRequestException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error getting saccos for team {teamId}: {ex.Message}", ex);
            }
        }

        public async Task<SaccoDTO> GetSaccoByTheirIdAsync(string saccoId)
        {
            try
            {
                var resp = await _httpClient.GetAsync($"/api/auth/saccos/{saccoId}");
                if (!resp.IsSuccessStatusCode)
                {
                    var errorContent = await resp.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Failed to get sacco by ID {saccoId}. Status: {resp.StatusCode}. Error: {errorContent}");
                }

                await using var stream = await resp.Content.ReadAsStreamAsync();
                var sacco = await JsonSerializer.DeserializeAsync<SaccoDTO>(stream, _jsonOpts)
                            ?? throw new InvalidDataException($"Deserialized sacco for {saccoId} is null.");

                return sacco;
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException($"JSON deserialization error for sacco {saccoId}: {ex.Message}", ex);
            }
            catch (HttpRequestException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error getting sacco by ID {saccoId}: {ex.Message}", ex);
            }
        }

    }
}
