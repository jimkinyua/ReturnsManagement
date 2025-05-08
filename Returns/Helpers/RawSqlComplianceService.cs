using Microsoft.Data.SqlClient;
using Returns.DTOs.Compliance;
using Returns.Helpers.Interfaces;
using System.Data;

namespace Returns.Helpers
{
    public class RawSqlComplianceService : IComplianceService
    {

        private readonly string _connectionString;
        private readonly ILogger<RawSqlComplianceService> _logger;

        public RawSqlComplianceService(IConfiguration configuration, ILogger<RawSqlComplianceService> logger)
        {
            _connectionString = configuration.GetConnectionString("IdentityDbConnection");
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
                            u.TeamName,
                            u.TeamRole,
                            r.Name  AS RoleName,
                            u.TeamId
                        FROM UserSaccos        us
                        JOIN AspNetUsers       u  ON u.Id = CAST(us.UserId AS nvarchar(450))
                        JOIN AspNetUserRoles   ur ON ur.UserId = u.Id
                        JOIN AspNetRoles       r  ON r.Id = ur.RoleId
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
                FROM [IdentityDatabase].[dbo].[Saccos]";

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
                                    TeamId = reader["TeamId"]?.ToString() ?? string.Empty,
                                    TeamName = reader["TeamName"]?.ToString() ?? string.Empty
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
                FROM [IdentityDatabase].[dbo].[Saccos]
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
                            u.TeamName,
                            u.TeamRole
                        FROM AspNetUsers AS u
                        INNER JOIN AspNetRoles AS r
                            ON r.Id = CONVERT(NVARCHAR(450), u.Role)
                        WHERE 
                            u.TeamId    = @teamId AND u.TeamRole = 1;  ";
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
                        FROM [IdentityDatabase].dbo.AspNetUsers U
                        INNER JOIN [IdentityDatabase].dbo.AspNetRoles R
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

        public Task<SasraRoleDetails?> GetRoleDetails(string RoleId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = @"  
                       SELECT TOP (1) [Id],[Name]
                        FROM [AspNetRoles] as R
                        WHERE R.Id = @RoleId
                        ";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add(new SqlParameter("@RoleId", SqlDbType.NVarChar) { Value = RoleId });
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var officer = new SasraRoleDetails
                                {
                                    RoleName = reader["Name"]?.ToString() ?? string.Empty,
                                    RoleId = reader["Id"]?.ToString() ?? string.Empty,
                                };
                                return Task.FromResult<SasraRoleDetails?>(officer);
                            }
                            return Task.FromResult<SasraRoleDetails?>(null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving team lead for Team ID {TeamId}", RoleId);
                return Task.FromResult<SasraRoleDetails?>(null);
            }
        }


        }
}
