using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Returns.Helpers
{
    public static class TokenHelper
    {
        private static JwtSecurityToken ExtractTokenFromRequest(HttpRequest request)
        {
            var token = request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            //var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImEwMzQxMDA4LWQ5ZDItNDEyYy04N2QxLTEyY2EwYjFlMDdiZCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6Inp1dnlmZXRvYmlAbWFpbGluYXRvci5jb20iLCJTYWNjb0lkIjoiMiIsIlNhY2NvTmFtZSI6IkNhbWVyb24gSGFyZGluIiwiU2FjY29UeXBlIjoiTm9uRGVwb3NpdFRha2luZyIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNlbyIsImV4cCI6MTc0MTQ0ODg0NywiaXNzIjoic2FzcmEuY29tIiwiYXVkIjoic2FzcmEuY29tIn0.1sWQTw_xdSp9clsuCgnPVLAAedipZRwpwqQFgDH3i9I"; //request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return new JwtSecurityTokenHandler().ReadJwtToken(token);
        }

        internal static string GetUserIdFromToken(HttpRequest request)
        {
            var token = ExtractTokenFromRequest(request);
            var userId = ((JwtSecurityToken)token).Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            return userId.ToString();
        }
        internal static LoggedInEntity GetLoggedInSaccoFromCurrentRequest(HttpRequest request)
        {
            var token = ExtractTokenFromRequest(request);
            var userId = ((JwtSecurityToken)token).Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var SaccoId = ((JwtSecurityToken)token).Claims.FirstOrDefault(c => c.Type == "SaccoId")?.Value;
            var SaccoName = ((JwtSecurityToken)token).Claims.FirstOrDefault(c => c.Type == "SaccoName")?.Value;
            var SaccoType = ((JwtSecurityToken)token).Claims.FirstOrDefault(c => c.Type == "SaccoType")?.Value;
            var EmailAddress = ((JwtSecurityToken)token).Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            //var SaccoType = "NonDepositTaking"; // Hardcoded for now

            // Map SaccoType names to their corresponding IDs
            var saccoTypeId = SaccoType switch
            {
                "DepositTaking" => "0",
                "NonDepositTaking" => "1",
                _ => "0" // Default or unknown type
            };

            return new LoggedInEntity
            {
                SaccoId = SaccoId,
                SaccoName = SaccoName,
                SaccoType = saccoTypeId,
                EmailAddress = EmailAddress,
                UserId = userId
            };
        }

        public class LoggedInEntity
        {
            public string SaccoName = "";
            public string SaccoType = "";
            public string SaccoId = "";
            public string EmailAddress = "";
            public string UserId = "";
        }
    }
}
