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
                            u.TeamRole
                        FROM [UserSaccos] us
                        INNER JOIN [AspNetUsers] u ON us.UserId = u.Id
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
    }
}
