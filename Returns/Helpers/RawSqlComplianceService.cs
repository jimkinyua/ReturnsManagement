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
	                        R.Name as RoleName,
	                        u.TeamId
                        FROM [UserSaccos] us
                        INNER JOIN [AspNetUsers] u ON us.UserId = u.Id
                        INNER JOIN [AspNetRoles] R ON R.Id = u .Role
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

        public Task<TeamLead?> GetTeamLead(string teamId)
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
                       FROM [AspNetUsers] u  
                       INNER JOIN [AspNetRoles] R ON R.Id = u.Role  
                       WHERE u.TeamId = @teamId AND u.TeamRole = 1";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add(new SqlParameter("@teamId", SqlDbType.NVarChar) { Value = teamId });
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var officer = new TeamLead
                                {
                                    Id = reader["Id"]?.ToString()??string.Empty,
                                    FullName = reader["FullName"]?.ToString() ?? string.Empty,
                                    Email = reader["Email"]?.ToString() ?? string.Empty,
                                    TeamName = reader["TeamName"]?.ToString() ?? string.Empty,
                                    TeamRole = reader["TeamRole"]?.ToString() ?? string.Empty,
                                };
                                return Task.FromResult<TeamLead?>(officer);
                            }
                            return Task.FromResult<TeamLead?>(null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving team lead for Team ID {TeamId}", teamId);
                return Task.FromResult<TeamLead?>(null);
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
                       SELECT TOP (1)
                            u.Id,
                            u.FullName,
                            u.Email,
                            u.TeamName,
                            u.TeamRole,
	                        R.Name as RoleName,
	                        u.TeamId
                        FROM [UserSaccos] us
                        INNER JOIN [AspNetUsers] u ON us.UserId = u.Id
                        INNER JOIN [AspNetRoles] R ON R.Id = @RoleId
                        ";
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
                _logger.LogError(ex, "Error retrieving team lead for Team ID {TeamId}", RoleId);
                return Task.FromResult<SasraUser?>(null);
            }
        }

       
    }
}
