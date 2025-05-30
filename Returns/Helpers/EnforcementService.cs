using Returns.DTOs.Compliance;
using Returns.DTOs.Enforcement;
using System.Net.Http.Headers;

namespace Returns.Helpers
{
    public class EnforcementService : IEnforcementService
    {
        private readonly HttpClient _http;
        public EnforcementService(HttpClient http) => _http = http;

        public async Task SubmitCaseAsync(EnforcementCaseRequestDTO dto, string bearerToken, CancellationToken ct = default)
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(dto.Title), "title");
            form.Add(new StringContent(dto.Description), "description");
            form.Add(new StringContent(dto.SaccoId.ToString()), "saccoId");
            form.Add(new StringContent(dto.SaccoName), "saccoName");
            form.Add(new StringContent(dto.Source), "source");
            form.Add(new StringContent(dto.SourceReferenceNo), "sourceReferenceNo");
            form.Add(new StringContent(dto.Classification), "classification");
            form.Add(new StringContent(dto.DateRequested.ToString("yyyy-MM-ddTHH:mm:ss.fffffff")), "dateRequested");
            form.Add(new StringContent(dto.Remarks), "remarks");

            if (dto.SupportingFile is { Length: > 0 })              // null-check & non-empty
            {
                var streamContent = new StreamContent(dto.SupportingFile.OpenReadStream());
                streamContent.Headers.ContentType =
                    string.IsNullOrWhiteSpace(dto.SupportingFile.ContentType)
                        ? new MediaTypeHeaderValue("application/octet-stream")
                        : new MediaTypeHeaderValue(dto.SupportingFile.ContentType);

                form.Add(streamContent, "SupportingDocuments", dto.SupportingFile.FileName);
            }


            using var req = new HttpRequestMessage( HttpMethod.Post, "https://sasra-backend.agilebiz.co.ke/gateway/api/enforcement/cases/enforcementcase-request")
            {
                Content = form
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            using var resp = await _http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException(
                    $"Enforcement API returned {(int)resp.StatusCode}: {body}");
            }

        }
    }
}
