using iText.Commons.Utils;
using iText.Forms.Form.Element;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Compliance;
using Returns.DTOs.Returns;
using Returns.DTOs.Returns.Returns_Submission;
using Returns.DTOs.Returns_Submission.DT;
using Returns.Helpers.Enums;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;
using System.Text.Json;
using static Returns.Helpers.TokenHelper;

namespace Returns.Helpers
{
    public class AmendmentService : IAmendmentService
    {
        private readonly ReturnsDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IComplianceService _complianceService;
        private readonly ILogger<AmendmentService> _logger;
        private readonly IReturnSubmissionService _returnSubmissionService;
        private readonly IReturnAmendmentPolicy _returnAmendmentPolicy;
        private readonly IExcelParser _excelParser;

        public AmendmentService(
            ReturnsDbContext context,
            IEmailService emailService,
            IComplianceService complianceService,
            ILogger<AmendmentService> logger,
            IReturnSubmissionService returnSubmissionService,
            IReturnAmendmentPolicy returnAmendmentPolicy,
            IExcelParser excelParser)
        {
            _context = context;
            _complianceService = complianceService;
            _emailService = emailService;
            _logger = logger;
            _returnSubmissionService = returnSubmissionService;
            _returnAmendmentPolicy = returnAmendmentPolicy;
            _excelParser = excelParser;
        }
        public async Task<ReturnSubmission?> GetSubmittedSubmissionsUsingExpectedIdAsync(string submissionId, string saccoId, Boolean Filing = false)
        {
            var submission = await _context.ReturnSubmissions
                .Include(s => s.ExpectedReturn)
                    .ThenInclude(er => er.ReturnForm)
                .Where(s => s.Status == SubmissionStatus.Submitted.ToString())
                .FirstOrDefaultAsync(s => s.ExpectedReturnId == submissionId);

            if (submission == null)
            {
                _logger.LogWarning("Return submission {SubmissionId} not found.", submissionId);
                if (Filing == true)
                {
                    return null;
                }
                throw new InvalidOperationException("Return submission not found.");
            }

            /* if (saccoId != submission.SaccoId)
             {
                 _logger.LogWarning("Unauthorized attempt to access submission {SubmissionId} by SACCO {SaccoId}", submissionId, saccoId);
                 throw new UnauthorizedAccessException("You do not have permission to access this submission.");
             }*/

            return submission;
        }


        public async Task<ReturnSubmission?> GetSubmissionUsingExpectedIdAsync(string submissionId, string saccoId, Boolean Filing = false)
        {
            var submission = await _context.ReturnSubmissions
                .Include(s => s.ExpectedReturn)
                    .ThenInclude(er => er.ReturnForm)
                .FirstOrDefaultAsync(s => s.ExpectedReturnId == submissionId);

            if (submission == null)
            {
                _logger.LogWarning("Return submission {SubmissionId} not found.", submissionId);
                if (Filing == true)
                {
                    return null;
                }
                throw new InvalidOperationException("Return submission not found.");
            }

            /* if (saccoId != submission.SaccoId)
             {
                 _logger.LogWarning("Unauthorized attempt to access submission {SubmissionId} by SACCO {SaccoId}", submissionId, saccoId);
                 throw new UnauthorizedAccessException("You do not have permission to access this submission.");
             }*/

            return submission;
        }

        public async Task<ReturnSubmission> GetSubmissionUsingReturnIdAsync(string submissionId, string saccoId)
        {
            var submission = await _context.ReturnSubmissions
                .Include(s => s.ExpectedReturn)
                .ThenInclude(er => er.ReturnForm)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
            {
                _logger.LogWarning("Return submission {SubmissionId} not found.", submissionId);
                throw new InvalidOperationException("Return submission not found.");
            }

            /*  if (saccoId != submission.SaccoId)
              {
                  _logger.LogWarning("Unauthorized attempt to access submission {SubmissionId} by SACCO {SaccoId}", submissionId, saccoId);
                  throw new UnauthorizedAccessException("You do not have permission to access this submission.");
              }*/

            return submission;
        }

        private async Task<(string FileUrl, bool ParseSuccess, string ContentsJson, string ParseErrorsJson)> ParseAndSaveFileAsync(IFormFile formFile, FormCategory category, string saccoTypeId, string submissionId)
        {
            string fileUrl;
            // If category is "Other", skip validation and parsing, just save any file (e.g., board minutes, PDFs, or other unstructured docs)
            if (category == FormCategory.Other)
            {
                fileUrl = await FormsHelper.SaveFileAsync(formFile, "Drafts");
                var FileUrlJson = JsonSerializer.Serialize(new { fileUrl });
                return (fileUrl, true, FileUrlJson, null);
            }

            // For non-"Other" categories, validate as Excel and parse
            if (!FormsHelper.IsValidExcelFile(formFile))
            {
                _logger.LogWarning("Invalid Excel file provided for submission {SubmissionId}.", submissionId);
                throw new InvalidOperationException("Invalid Excel file format. Only .xlsx files are supported.");
            }

            var parseResult = await _excelParser.ParseAsync(formFile, category, saccoTypeId);
            fileUrl = await FormsHelper.SaveFileAsync(formFile, "Drafts");
            var parseSuccess = parseResult.Success;
            var contentsJson = parseResult.Success
                ? JsonSerializer.Serialize(parseResult.Rows.Select(row => row.ToEntity()),
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                : null;
            var parseErrorsJson = JsonSerializer.Serialize(parseResult.Errors);
            if (!parseResult.Success)
            {
                _logger.LogWarning("Failed to parse Excel file for submission {SubmissionId}: {Errors}",
                    submissionId, string.Join(", ", parseResult.Errors));
                throw new InvalidOperationException($"Failed to parse Excel file: {string.Join(", ", parseResult.Errors)}");
            }
            return (fileUrl, parseSuccess, contentsJson, parseErrorsJson);
        }



        private async Task<AmendmentRequest?> CheckExistingAmendmentRequestAsync(string submissionId)
        {
            return await _context.AmendmentRequests
                .Where(r => r.ReturnSubmissionId == submissionId
                            && (r.Status == AmendmentStatus.PendingAdminApproval || r.Status == AmendmentStatus.PendingSaccoResponse))
                .OrderByDescending(r => r.RequestedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<AmendmentRequest> CreateAmendRequestForSacco(AmendmentRequestDTO dto, LoggedInEntity loggedInEntity)
        {


            if (dto.FormFile == null)
            {
                throw new ArgumentException("An Excel file is required for amendment requests.", nameof(dto.FormFile));
            }

            // Check for existing amendment requests
            var existingRequest = await CheckExistingAmendmentRequestAsync(dto.SubmissionId);
            if (existingRequest != null)
            {
                if (existingRequest.IsAdminInitiated)
                {
                    throw new InvalidOperationException("An admin-initiated amendment request exists. Please Respond to It.");
                }
                else
                {
                    throw new InvalidOperationException("An amendment request is already pending or approved for this submission.");
                }
            }

            var today = DateTime.Now.Date;
            if (_returnAmendmentPolicy.CanAutoAmend(today))
            {
                throw new InvalidOperationException("Amendment requests are not allowed on or before the 15th. Use direct submission instead.");
            }

            // Retrieve and validate submission
            var submission = await GetSubmissionUsingReturnIdAsync(dto.SubmissionId, loggedInEntity.SaccoId);

            // Parse and save file
            var (fileUrl, parseSuccess, contentsJson, parseErrorsJson) = await ParseAndSaveFileAsync(
                dto.FormFile,
                (FormCategory)submission.ExpectedReturn.ReturnForm.Category,
                submission.ExpectedReturn.ReturnForm.SaccoTypeId,
                dto.SubmissionId);

            var newRequest = new AmendmentRequest
            {
                ExpectedReturnId = submission.ExpectedReturnId,
                ReturnSubmissionId = dto.SubmissionId,
                SaccoType = loggedInEntity.SaccoType,
                SaccoName = loggedInEntity.SaccoName,
                SaccoId = submission.SaccoId,
                RequestedById = loggedInEntity.UserId,
                RequestedAt = DateTime.Now,
                Reason = dto.AmendmentReason.Trim(),
                Status = AmendmentStatus.PendingAdminApproval,
                FileUrl = fileUrl,
                ParseSuccess = parseSuccess,
                ContentsJson = contentsJson,
                ParseErrorsJson = parseErrorsJson
            };

            await _context.AmendmentRequests.AddAsync(newRequest);
            await _context.SaveChangesAsync();

            return newRequest;
        }

        public async Task<AmendmentRequest> RespondToAdminAmendmentRequestAsync(SaccoAmendmentResponseDTO dto, LoggedInEntity loggedInEntity)
        {


            // Retrieve and validate submission

            // Check for admin-initiated amendment request
            var existingRequest = await _context.AmendmentRequests
                 .Where(r =>
                     r.Id == dto.ResubmissionRequestId
                  && r.Status == AmendmentStatus.PendingSaccoResponse    // direct enum comparison
                  && r.IsAdminInitiated                     // bool comparison
                 )
                 .OrderByDescending(r => r.RequestedAt)
                 .FirstOrDefaultAsync();


            if (existingRequest == null)
            {
                throw new InvalidOperationException("No admin-initiated amendment request found for this submission.");
            }

            var submission = await GetSubmissionUsingExpectedIdAsync(existingRequest.ExpectedReturnId, loggedInEntity.SaccoId);


            // Parse and save file
            var (fileUrl, parseSuccess, contentsJson, parseErrorsJson) = await ParseAndSaveFileAsync(
                dto.FormFile,
                (FormCategory)submission.ExpectedReturn.ReturnForm.Category,
                submission.ExpectedReturn.ReturnForm.SaccoTypeId,
                dto.ResubmissionRequestId);

            // Update existing amendment request
            existingRequest.FileUrl = fileUrl;
            existingRequest.ParseSuccess = parseSuccess;
            existingRequest.ContentsJson = contentsJson;
            existingRequest.ParseErrorsJson = parseErrorsJson;
            existingRequest.Status = AmendmentStatus.Responded; // Mark as Responded after processing

            _context.AmendmentRequests.Entry(existingRequest).State = EntityState.Modified;
            _context.AmendmentRequests.Update(existingRequest);
            // Process submission directly
            var newReturnDto = new NewReturnDTO
            {
                FormUploads = new List<ReturnFormUploadDTO>
                {
                    new ReturnFormUploadDTO
                    {
                        ExpectedReturnId = submission.ExpectedReturnId,
                        formFile = dto.FormFile
                    }
                }
            };

            var result = await _returnSubmissionService.UploadDraftAsync(newReturnDto, loggedInEntity, true);
            if (result.Any(r => r.Status == SubmissionStatus.Failed))
            {
                throw new InvalidOperationException($"Failed to process amendment: {string.Join(", ", result.SelectMany(r => r.Messages))}");
            }

            await _context.SaveChangesAsync();


            return existingRequest;
        }

        public async Task<AmendmentRequest> CreateAdminAmendmentRequestAsync(AdminAmendmentRequestDTO dto, LoggedInEntity admin)
        {

            // Retrieve and validate submission
            var submission = await GetSubmissionUsingReturnIdAsync(dto.ReturnSubmissionId, admin.SaccoId);
            var saccoDetails = await _complianceService.GetSaccoByTheirIdAsync(submission.SaccoId);
            // Check for existing amendment requests
            var existingRequest = await CheckExistingAmendmentRequestAsync(dto.ReturnSubmissionId);
            if (existingRequest != null)
            {
                throw new InvalidOperationException("An amendment request is already pending or approved for this submission.");
            }

            // Create new amendment request
            var newRequest = new AmendmentRequest
            {
                ExpectedReturnId = submission.ExpectedReturnId,
                SaccoType = submission.ExpectedReturn.ReturnForm.SaccoTypeId,
                SaccoName = saccoDetails.SaccoName,
                ReturnSubmissionId = dto.ReturnSubmissionId,
                SaccoId = submission.SaccoId,
                RequestedById = admin.UserId,
                RequestedAt = DateTime.UtcNow,
                Reason = dto.Reason.Trim(),
                Status = AmendmentStatus.PendingSaccoResponse,
                FileUrl = string.Empty,
                ParseSuccess = true,
                ContentsJson = null,
                ParseErrorsJson = null,
                IsAdminInitiated = true
            };

            await _context.AmendmentRequests.AddAsync(newRequest);
            await _context.SaveChangesAsync();

            // Notify SACCO

            _logger.LogInformation("Admin amendment request {RequestId} created for submission {ReturnSubmissionId} by admin {UserId}", newRequest.Id, dto.ReturnSubmissionId, admin.UserId);

            return newRequest;
        }
        public async Task<IList<PendingAmendmentRequestDTO>> GetAmendmentRequestsPendingSaccoResponseAsync()
        {

            var requests = await _context.AmendmentRequests
            .Include(r => r.ReturnSubmission)
                .ThenInclude(rs => rs.ExpectedReturn)
                    .ThenInclude(er => er.ReturnForm)
            .Include(r => r.ReturnSubmission)
                .ThenInclude(rs => rs.ExpectedReturn)
                    .ThenInclude(er => er.Period)
                        .ThenInclude(p => p.ReportingYear)
                .Where(r => r.Status == AmendmentStatus.PendingSaccoResponse)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();



            var pendingRequests = new List<PendingAmendmentRequestDTO>();

            foreach (var r in requests)
            {
                var Year = r.ReturnSubmission.ExpectedReturn.Period.ReportingYear.Year;
                var Period = r.ReturnSubmission.ExpectedReturn.Period.Name;
                var PeriodStart = r.ReturnSubmission.ExpectedReturn.Period.StartDate.ToLongDateString();
                var PeriodEnd = r.ReturnSubmission.ExpectedReturn.Period.EndDate.ToLongDateString();
                var OriginalSubmittedAt = r.ReturnSubmission.CreatedAt.ToLongDateString();

                var dto = new PendingAmendmentRequestDTO
                {
                    Id = r.Id,
                    ExpectedReturnId = r.ExpectedReturnId,
                    ReturnSubmissionId = r.ReturnSubmissionId,
                    SaccoId = r.SaccoId,
                    SaccoType = r.SaccoType,
                    SaccoName = r.SaccoName,
                    RequestedById = r.RequestedById,
                    RequestedAt = r.RequestedAt.ToLongDateString(),
                    Reason = r.Reason,
                    Status = r.Status,
                    ReturnType = r.ReturnSubmission.ExpectedReturn.ReturnForm != null
                        ? r.ReturnSubmission.ExpectedReturn.ReturnForm.Category.ToString()
                        : null,
                    OriginalYear = Year.ToString(),
                    OriginalPeriod = Period,
                    PeriodStart = PeriodStart,
                    PeriodEnd = PeriodEnd,
                    OriginalSubmittedAt = OriginalSubmittedAt
                };

                pendingRequests.Add(dto);
            }

            return pendingRequests;
        }

        public async Task<AmendmentRequestDetailsDTO> GetAmendmentRequestDetailsAsync(string requestId, LoggedInEntity admin)
        {
            var request = await _context.AmendmentRequests
                .Include(r => r.ReturnSubmission)
                .ThenInclude(er => er.ExpectedReturn)
                .ThenInclude(er => er.ReturnForm)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == requestId);
            if (request == null)
            {
                _logger.LogWarning("Amendment request {RequestId} not found.", requestId);
                throw new InvalidOperationException("Amendment request not found.");
            }
            List<string> parseErrors = new List<string>();
            List<object> rows = new List<object>();
            try
            {
                if (!string.IsNullOrEmpty(request.ParseErrorsJson))
                {
                    parseErrors = JsonSerializer.Deserialize<List<string>>(request.ParseErrorsJson) ?? new List<string>();
                }
                if (!string.IsNullOrEmpty(request.ContentsJson))
                {
                    if (request.ParseSuccess)
                    {
                        if (request.ReturnSubmission.ExpectedReturn.ReturnForm.Category == FormCategory.Other)
                        {
                            var contentsObj = JsonSerializer.Deserialize<Dictionary<string, string>>(request.ContentsJson);
                            var fileUrl = contentsObj?.GetValueOrDefault("fileUrl");
                            if (!string.IsNullOrEmpty(fileUrl))
                            {
                                rows.Add(new { FileUrl = fileUrl });
                            }
                        }
                        else
                        {
                            rows = JsonSerializer.Deserialize<List<object>>(request.ContentsJson) ?? new List<object>();
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize JSON for amendment request {RequestId}", requestId);
                parseErrors.Add("Failed to deserialize amendment request contents.");
            }
            // Get current child data based on ReturnType
            List<object> currentData = new List<object>();
            var returnType = request.ReturnSubmission.ExpectedReturn.ReturnForm != null
                ? (FormCategory?)request.ReturnSubmission.ExpectedReturn.ReturnForm.Category
                : null;
            if (returnType.HasValue)
            {
                if (returnType.Value == FormCategory.Other)
                {
                    var currentFileUrl = request.ReturnSubmission?.FileUrl;
                    if (!string.IsNullOrEmpty(currentFileUrl))
                    {
                        currentData.Add(new { FileUrl = currentFileUrl });
                    }
                }
                else
                {
                    currentData = await GetCurrentChildDataAsync(request.ReturnSubmission, returnType.Value);
                }
            }
            var details = new AmendmentRequestDetailsDTO
            {
                Id = request.Id,
                ExpectedReturnId = request.ExpectedReturnId,
                ReturnSubmissionId = request.ReturnSubmissionId,
                SaccoId = request.SaccoId,
                SaccoType = request.SaccoType,
                RequestedById = request.RequestedById,
                RequestedAt = request.RequestedAt,
                ReviewedById = request.ReviewedById,
                ReviewedAt = request.ReviewedAt,
                Reason = request.Reason,
                Status = request.Status,
                FileUrl = request.FileUrl,
                Rows = rows,
                CurrentData = currentData,
                ReturnType = returnType
            };
            return details;
        }

        /// <summary>
        /// Fetches current child data based on the form category
        /// </summary>
        private async Task<List<object>> GetCurrentChildDataAsync(ReturnSubmission returnSubmission, FormCategory formCategory)
        {
            var currentData = new List<object>();

            try
            {
                // Get the SACCO type from the amendment request context
                var saccoType = returnSubmission.ExpectedReturn.ReturnForm.SaccoTypeId;

                switch (formCategory)
                {
                    case FormCategory.CapitalAdequacy:
                        if (saccoType == "0") // DT Returns
                        {
                            var capitalAdequacyData = await _context.DTCapitalAdequacyReturns
                                .Where(r => r.ReturnSubmission.SaccoId == returnSubmission.SaccoId && r.IsCurrent)
                                .OrderByDescending(r => r.CreatedAt)
                                .Take(1)
                                .ToListAsync();
                            // Serialize with camelCase to match rows format
                            var serializedData = JsonSerializer.Serialize(capitalAdequacyData, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            });
                            var deserializedData = JsonSerializer.Deserialize<List<object>>(serializedData);
                            currentData.AddRange(deserializedData ?? new List<object>());
                        }
                        else if (saccoType == "1") // NWDT Returns
                        {
                            var nwdtCapitalAdequacyData = await _context.NWDTCapitalAdequacyReturns
                                .Where(r => r.ReturnSubmission.SaccoId == returnSubmission.SaccoId && r.IsCurrent)
                                .OrderByDescending(r => r.CreatedAt)
                                .Take(1)
                                .ToListAsync();
                            // Serialize with camelCase to match rows format
                            var serializedData = JsonSerializer.Serialize(nwdtCapitalAdequacyData, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            });
                            var deserializedData = JsonSerializer.Deserialize<List<object>>(serializedData);
                            currentData.AddRange(deserializedData ?? new List<object>());
                        }
                        break;

                    case FormCategory.LiquidityStatement:
                        if (saccoType == "0") // DT Returns
                        {
                            var liquidityData = await _context.DTLiquidityReturns
                                .Where(r => r.ReturnSubmission.SaccoId == returnSubmission.SaccoId && r.IsCurrent)
                                .OrderByDescending(r => r.CreatedAt)
                                .Take(1)
                                .ToListAsync();
                            // Serialize with camelCase to match rows format
                            var serializedData = JsonSerializer.Serialize(liquidityData, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            });
                            var deserializedData = JsonSerializer.Deserialize<List<object>>(serializedData);
                            currentData.AddRange(deserializedData ?? new List<object>());
                        }
                        else if (saccoType == "1") // NWDT Returns
                        {
                            var nwdtLiquidityData = await _context.NDWTLiquidityReturns
                                .Where(r => r.ReturnSubmission.SaccoId == returnSubmission.SaccoId && r.IsCurrent)
                                .OrderByDescending(r => r.CreatedAt)
                                .Take(1)
                                .ToListAsync();
                            // Serialize with camelCase to match rows format
                            var serializedData = JsonSerializer.Serialize(nwdtLiquidityData, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            });
                            var deserializedData = JsonSerializer.Deserialize<List<object>>(serializedData);
                            currentData.AddRange(deserializedData ?? new List<object>());
                        }
                        break;

                    case FormCategory.RiskClassification:
                        if (saccoType == "0") // DT Returns
                        {
                            var riskData = await _context.DTRiskClassificationReturns
                                .Where(r => r.ReturnSubmission.SaccoId == returnSubmission.SaccoId && r.IsCurrent)
                                .OrderByDescending(r => r.CreatedAt)
                                .ToListAsync();
                            // Serialize with camelCase to match rows format
                            var serializedData = JsonSerializer.Serialize(riskData, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            });
                            var deserializedData = JsonSerializer.Deserialize<List<object>>(serializedData);
                            currentData.AddRange(deserializedData ?? new List<object>());
                        }
                        else if (saccoType == "1") // NWDT Returns
                        {
                            var nwdtRiskData = await _context.NWDTRiskClassificationReturns
                                .Where(r => r.ReturnSubmission.SaccoId == returnSubmission.SaccoId && r.IsCurrent)
                                .OrderByDescending(r => r.CreatedAt)
                                .ToListAsync();
                            // Serialize with camelCase to match rows format
                            var serializedData = JsonSerializer.Serialize(nwdtRiskData, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            });
                            var deserializedData = JsonSerializer.Deserialize<List<object>>(serializedData);
                            currentData.AddRange(deserializedData ?? new List<object>());
                        }
                        break;

                    case FormCategory.InvestmentReturn:
                        if (saccoType == "0") // DT Returns
                        {
                            var investmentData = await _context.DTInvestmentReturns
                                .Where(r => r.ReturnSubmission.SaccoId == returnSubmission.SaccoId && r.IsCurrent)
                                .OrderByDescending(r => r.CreatedAt)
                                .ToListAsync();
                            // Serialize with camelCase to match rows format
                            var serializedData = JsonSerializer.Serialize(investmentData, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            });
                            var deserializedData = JsonSerializer.Deserialize<List<object>>(serializedData);
                            currentData.AddRange(deserializedData ?? new List<object>());
                        }
                        else if (saccoType == "1") // NWDT Returns
                        {
                            var nwdtInvestmentData = await _context.NWDTInvestmentReturns
                                .Where(r => r.ReturnSubmission.SaccoId == returnSubmission.SaccoId && r.IsCurrent)
                                .OrderByDescending(r => r.CreatedAt)
                                .ToListAsync();
                            // Serialize with camelCase to match rows format
                            var serializedData = JsonSerializer.Serialize(nwdtInvestmentData, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            });
                            var deserializedData = JsonSerializer.Deserialize<List<object>>(serializedData);
                            currentData.AddRange(deserializedData ?? new List<object>());
                        }
                        break;

                    case FormCategory.FinancialPosition:
                        if (saccoType == "0") // DT Returns
                        {
                            var financialPositionData = await _context.DTFinancialPositionReturns
                                .Where(r => r.ReturnSubmission.SaccoId == returnSubmission.SaccoId && r.IsCurrent)
                                .OrderByDescending(r => r.CreatedAt)
                                .ToListAsync();
                            // Serialize with camelCase to match rows format
                            var serializedData = JsonSerializer.Serialize(financialPositionData, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            });
                            var deserializedData = JsonSerializer.Deserialize<List<object>>(serializedData);
                            currentData.AddRange(deserializedData ?? new List<object>());
                        }
                        else if (saccoType == "1") // NWDT Returns
                        {
                            var nwdtFinancialPositionData = await _context.NWDTFinancialPositionReturns
                                .Include(r => r.ReturnSubmission)
                                .Where(r => r.ReturnSubmission.SaccoId == returnSubmission.SaccoId && r.IsCurrent)
                                .OrderByDescending(r => r.CreatedAt)
                                .Take(1)
                                .ToListAsync();
                            // Serialize with camelCase to match rows format
                            var serializedData = JsonSerializer.Serialize(nwdtFinancialPositionData, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            });
                            var deserializedData = JsonSerializer.Deserialize<List<object>>(serializedData);
                            currentData.AddRange(deserializedData ?? new List<object>());
                        }
                        break;

                    case FormCategory.StatementOfComprehensiveIncome:
                        if (saccoType == "0") // DT Returns
                        {
                            var comprehensiveIncomeData = await _context.DTComprehensiveIncomeReturns
                                .Include(r => r.ReturnSubmission)
                                .Where(r => r.ReturnSubmission.SaccoId == returnSubmission.SaccoId && r.IsCurrent)
                                .OrderByDescending(r => r.CreatedAt)
                                .Take(1)
                                .ToListAsync();
                            // Serialize with camelCase to match rows format
                            var serializedData = JsonSerializer.Serialize(comprehensiveIncomeData, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            });
                            var deserializedData = JsonSerializer.Deserialize<List<object>>(serializedData);
                            currentData.AddRange(deserializedData ?? new List<object>());
                        }
                        else if (saccoType == "1") // NWDT Returns
                        {
                            var nwdtComprehensiveIncomeData = await _context.NWDTComprehensiveIncomeReturns
                                .Include(r => r.ReturnSubmission)
                                .Where(r => r.ReturnSubmission.SaccoId == returnSubmission.SaccoId && r.IsCurrent)
                                .OrderByDescending(r => r.CreatedAt)
                                .Take(1)
                                .ToListAsync();
                            // Serialize with camelCase to match rows format
                            var serializedData = JsonSerializer.Serialize(nwdtComprehensiveIncomeData, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            });
                            var deserializedData = JsonSerializer.Deserialize<List<object>>(serializedData);
                            currentData.AddRange(deserializedData ?? new List<object>());
                        }
                        break;

                    case FormCategory.DepositReturn:
                        if (saccoType == "0") // DT Returns
                        {
                            var depositData = await _context.DepositReturns
                                .Include(r => r.ReturnSubmission)
                                .Where(r => r.ReturnSubmission.SaccoId == returnSubmission.SaccoId && r.IsCurrent)
                                .OrderByDescending(r => r.CreatedAt)
                                .ToListAsync();
                            // Serialize with camelCase to match rows format
                            var serializedData = JsonSerializer.Serialize(depositData, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            });
                            var deserializedData = JsonSerializer.Deserialize<List<object>>(serializedData);
                            currentData.AddRange(deserializedData ?? new List<object>());
                        }
                        else if (saccoType == "1") // NWDT Returns
                        {
                            var nwdtDepositData = await _context.NWDTDepositReturns
                                .Include(r => r.ReturnSubmission)
                                .Where(r => r.ReturnSubmission.SaccoId == returnSubmission.SaccoId && r.IsCurrent)
                                .OrderByDescending(r => r.CreatedAt)
                                .ToListAsync();
                            // Serialize with camelCase to match rows format
                            var serializedData = JsonSerializer.Serialize(nwdtDepositData, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            });
                            var deserializedData = JsonSerializer.Deserialize<List<object>>(serializedData);
                            currentData.AddRange(deserializedData ?? new List<object>());
                        }
                        break;

                    case FormCategory.Management:
                        var managementData = await _context.ManagementReturns
                            .Include(r => r.Return)
                            .Where(r => r.Return.SaccoId == returnSubmission.SaccoId && r.IsCurrent)
                            .OrderByDescending(r => r.CreatedAt)
                            .Take(1)
                            .ToListAsync();
                        // Serialize with camelCase to match rows format
                        var managementSerializedData = JsonSerializer.Serialize(managementData, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });
                        var managementDeserializedData = JsonSerializer.Deserialize<List<object>>(managementSerializedData);
                        currentData.AddRange(managementDeserializedData ?? new List<object>());
                        break;

                    case FormCategory.DailyLiquidity:
                        var dailyLiquidityData = await _context.DailyLiquidityReturns
                            .Include(r => r.Return)
                            .Where(r => r.Return.SaccoId == returnSubmission.SaccoId && r.IsCurrent)
                            .OrderByDescending(r => r.CreatedAt)
                            .Take(1)
                            .ToListAsync();
                        // Serialize with camelCase to match rows format
                        var dailyLiquiditySerializedData = JsonSerializer.Serialize(dailyLiquidityData, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });
                        var dailyLiquidityDeserializedData = JsonSerializer.Deserialize<List<object>>(dailyLiquiditySerializedData);
                        currentData.AddRange(dailyLiquidityDeserializedData ?? new List<object>());
                        break;

                    case FormCategory.SectoralLending:
                        var sectoralLendingData = await _context.SectoralLendingReports
                            .Include(r => r.ReturnSubmission)
                            .Where(r => r.ReturnSubmission.SaccoId == returnSubmission.SaccoId && r.IsCurrent)
                            .OrderByDescending(r => r.CreatedAt)
                            .Take(1)
                            .ToListAsync();
                        // Serialize with camelCase to match rows format
                        var sectoralLendingSerializedData = JsonSerializer.Serialize(sectoralLendingData, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });
                        var sectoralLendingDeserializedData = JsonSerializer.Deserialize<List<object>>(sectoralLendingSerializedData);
                        currentData.AddRange(sectoralLendingDeserializedData ?? new List<object>());
                        break;

                    case FormCategory.InsiderLending:
                        var insiderLendingData = await _context.InsiderLendingHeaders
                            .Include(r => r.Return)
                            .Where(r => r.Return.SaccoId == returnSubmission.SaccoId && r.IsCurrent)
                            .OrderByDescending(r => r.CreatedAt)
                            .Take(1)
                            .ToListAsync();
                        // Serialize with camelCase to match rows format
                        var insiderLendingSerializedData = JsonSerializer.Serialize(insiderLendingData, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });
                        var insiderLendingDeserializedData = JsonSerializer.Deserialize<List<object>>(insiderLendingSerializedData);
                        currentData.AddRange(insiderLendingDeserializedData ?? new List<object>());
                        break;

                    default:
                        // _logger.LogWarning("Unsupported form category {FormCategory} for sacco {SaccoId}", formCategory, saccoId);
                        break;
                }
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error fetching current child data for form category {FormCategory} and sacco {SaccoId}", formCategory, saccoId);
            }

            return currentData;
        }

        public async Task ReviewAmendmentRequest(string requestId, bool approve, string reviewerId)
        {
            var request = await _context.AmendmentRequests
                .Include(r => r.ExpectedReturn)
                .Include(r => r.ReturnSubmission)
                    .ThenInclude(r => r.ExpectedReturn)
                        .ThenInclude(er => er.ReturnForm)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                _logger.LogWarning("Amendment request {RequestId} not found.", requestId);
                throw new InvalidOperationException("Amendment request not found.");
            }

            if (request.Status != AmendmentStatus.PendingAdminApproval)
            {
                _logger.LogWarning("Amendment request {RequestId} is not in Pending status. Current status: {Status}", requestId, request.Status);
                throw new InvalidOperationException($"Amendment request is not pending. Current status: {request.Status}.");
            }

            _logger.LogInformation("Reviewing amendment request {RequestId}. Current status: {CurrentStatus}", requestId, request.Status);

            // Update the amendment request
            request.ReviewedById = reviewerId;
            request.ReviewedAt = DateTime.UtcNow;
            request.Status = approve ? AmendmentStatus.Approved : AmendmentStatus.Rejected;

            // Explicitly tell Entity Framework that the entity has been modified
            _context.Entry(request).State = EntityState.Modified;

            if (approve)
            {
                if (string.IsNullOrEmpty(request.FileUrl))
                {
                    _logger.LogWarning("Amendment request {RequestId} has no associated file.", requestId);
                    throw new InvalidOperationException("No file associated with the amendment request.");
                }
                if (!request.ParseSuccess)
                {
                    _logger.LogWarning("Amendment request {RequestId} has invalid contents: {Errors}", requestId, request.ParseErrorsJson);
                    throw new InvalidOperationException("Cannot approve request with invalid file contents.");
                }

                var dto = new NewReturnDTO
                {
                    FormUploads = new List<ReturnFormUploadDTO>
                {
                    new ReturnFormUploadDTO
                    {
                        ExpectedReturnId = request.ExpectedReturnId,
                        formFile = await FormsHelper.GetFileFromUrlAsync(request.FileUrl)
                    }
                }
                };

                var sacco = new LoggedInEntity
                {
                    SaccoId = request.SaccoId,
                    UserId = request.RequestedById,
                    SaccoType = request.ReturnSubmission.ExpectedReturn.ReturnForm?.SaccoTypeId ?? string.Empty
                };

                var result = await _returnSubmissionService.UploadDraftAsync(dto, sacco, true, false, request.FileUrl);
                if (result.Any(r => r.Status == SubmissionStatus.Failed))
                {
                    _logger.LogWarning("Failed to process approved amendment request {RequestId}: {Errors}", requestId, string.Join(", ", result.SelectMany(r => r.Messages)));
                    request.Status = AmendmentStatus.PendingAdminApproval; // Revert to Pending on failure
                    _context.Entry(request).State = EntityState.Modified; // Mark as modified again after status change
                    await _context.SaveChangesAsync(); // Save the revert immediately
                    throw new InvalidOperationException($"Failed to process approved submission: {string.Join(", ", result.SelectMany(r => r.Messages))}");
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Amendment request {RequestId} updated to {NewStatus} by reviewer {ReviewerId}", requestId, request.Status, reviewerId);
        }

        public async Task<IList<PendingAmendmentRequestDTO>> GetAmendmentRequestsPendingAdminApprovalAsync()
        {
            var requests = await _context.AmendmentRequests
           .Include(r => r.ReturnSubmission)
               .ThenInclude(rs => rs.ExpectedReturn)
                   .ThenInclude(er => er.ReturnForm)
           .Include(r => r.ReturnSubmission)
               .ThenInclude(rs => rs.ExpectedReturn)
                   .ThenInclude(er => er.Period)
                       .ThenInclude(p => p.ReportingYear)
            .Where(r => r.Status == AmendmentStatus.PendingAdminApproval)
           .OrderByDescending(r => r.RequestedAt)
                            .Select(r => new PendingAmendmentRequestDTO
                            {
                                Id = r.Id,
                                ExpectedReturnId = r.ExpectedReturnId,
                                ReturnSubmissionId = r.ReturnSubmissionId,
                                SaccoId = r.SaccoId,
                                RequestedById = r.RequestedById,
                                RequestedAt = r.RequestedAt.ToLongDateString(),
                                Reason = r.Reason,
                                Status = r.Status,
                                ReturnType = r.ReturnSubmission.ExpectedReturn.ReturnForm != null
                                    ? r.ReturnSubmission.ExpectedReturn.ReturnForm.Category.ToString()
                                    : null
                            })
                            .ToListAsync();

            return requests;
        }

        public async Task<IList<PendingAmendmentRequestDTO>> GetAdminInitiatedAmendmentRequestsAsync()
        {


            var requests = await _context.AmendmentRequests
                .Include(r => r.ReturnSubmission)
                    .ThenInclude(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.ReturnForm)
                .Include(r => r.ReturnSubmission)
                    .ThenInclude(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.Period)
                            .ThenInclude(p => p.ReportingYear)
                .Where(r => r.IsAdminInitiated)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();

            var adminRequests = new List<PendingAmendmentRequestDTO>();

            foreach (var r in requests)
            {
                var Year = r.ReturnSubmission.ExpectedReturn.Period.ReportingYear.Year;
                var Period = r.ReturnSubmission.ExpectedReturn.Period.Name;
                var PeriodStart = r.ReturnSubmission.ExpectedReturn.Period.StartDate.ToLongDateString();
                var PeriodEnd = r.ReturnSubmission.ExpectedReturn.Period.EndDate.ToLongDateString();
                var OriginalSubmittedAt = r.ReturnSubmission.CreatedAt.ToLongDateString();

                var dto = new PendingAmendmentRequestDTO
                {
                    Id = r.Id,
                    ExpectedReturnId = r.ExpectedReturnId,
                    ReturnSubmissionId = r.ReturnSubmissionId,
                    SaccoId = r.SaccoId,
                    SaccoType = r.SaccoType,
                    SaccoName = r.SaccoName,
                    RequestedById = r.RequestedById,
                    RequestedAt = r.RequestedAt.ToLongDateString(),
                    Reason = r.Reason,
                    Status = r.Status,
                    ReturnType = r.ReturnSubmission.ExpectedReturn.ReturnForm != null
                        ? r.ReturnSubmission.ExpectedReturn.ReturnForm.Category.ToString()
                        : null,
                    OriginalYear = Year.ToString(),
                    OriginalPeriod = Period,
                    PeriodStart = PeriodStart,
                    PeriodEnd = PeriodEnd,
                    OriginalSubmittedAt = OriginalSubmittedAt
                };

                adminRequests.Add(dto);
            }

            return adminRequests;
        }

        /// <summary>
        /// Gets all amendment requests for a specific SACCO (including past approved/rejected ones)
        /// </summary>
        public async Task<IList<PendingAmendmentRequestDTO>> GetSaccoAmendmentRequestsAsync(string saccoId)
        {
            var requests = await _context.AmendmentRequests
                .Include(r => r.ReturnSubmission)
                    .ThenInclude(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.ReturnForm)
                .Include(r => r.ReturnSubmission)
                    .ThenInclude(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.Period)
                            .ThenInclude(p => p.ReportingYear)
                .Where(r => r.SaccoId == saccoId)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();

            var saccoRequests = new List<PendingAmendmentRequestDTO>();

            foreach (var r in requests)
            {
                var Year = r.ReturnSubmission.ExpectedReturn.Period.ReportingYear.Year;
                var Period = r.ReturnSubmission.ExpectedReturn.Period.Name;
                var PeriodStart = r.ReturnSubmission.ExpectedReturn.Period.StartDate.ToLongDateString();
                var PeriodEnd = r.ReturnSubmission.ExpectedReturn.Period.EndDate.ToLongDateString();
                var OriginalSubmittedAt = r.ReturnSubmission.CreatedAt.ToLongDateString();

                var dto = new PendingAmendmentRequestDTO
                {
                    Id = r.Id,
                    ExpectedReturnId = r.ExpectedReturnId,
                    ReturnSubmissionId = r.ReturnSubmissionId,
                    SaccoId = r.SaccoId,
                    SaccoType = r.SaccoType,
                    SaccoName = r.SaccoName,
                    RequestedById = r.RequestedById,
                    RequestedAt = r.RequestedAt.ToLongDateString(),
                    Reason = r.Reason,
                    Status = r.Status,
                    ReturnType = r.ReturnSubmission.ExpectedReturn.ReturnForm != null
                        ? r.ReturnSubmission.ExpectedReturn.ReturnForm.Category.ToString()
                        : null,
                    OriginalYear = Year.ToString(),
                    OriginalPeriod = Period,
                    PeriodStart = PeriodStart,
                    PeriodEnd = PeriodEnd,
                    OriginalSubmittedAt = OriginalSubmittedAt
                };

                saccoRequests.Add(dto);
            }

            return saccoRequests;
        }
    }
}
