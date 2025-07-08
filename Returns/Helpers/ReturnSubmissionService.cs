using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Returns.Returns_Submission;
using Returns.DTOs.Returns_Submission.DT;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;
using System;
using static Returns.Helpers.Constants;

namespace Returns.Helpers
{
    public class ReturnSubmissionService : IReturnSubmissionService
    {
        private readonly ReturnsDbContext _context;
        public Task<IList<SubmissionResultDto>> UploadDraftAsync(NewReturnDTO dto, string SaccoType, string SaccoId)
        {
            var results = new List<SubmissionResultDto>();

            foreach (var item in dto.FormUploads)
            {
                var res = new SubmissionResultDto();
                res.FormFileName = item.formFile.FileName;
                if (item.formFile == null || item.formFile.Length == 0)
                {
                    res.Status = SubmissionStatus.Failed;
                    res.Messages.Add("Form file is required.");
                    results.Add(res);
                    continue;
                }
                var expected = _context.ExpectedReturns
               .Include(er => er.ReturnForm)
               .FirstOrDefaultAsync(er => er.Id == item.ExpectedReturnId).Result;

                if (expected == null || expected.ReturnForm.SaccoTypeId != SaccoType)
                {
                    res.Status = SubmissionStatus.Failed;
                    res.Messages.Add("Invalid form or not allowed for your Sacco type.");
                    results.Add(res);
                    continue;
                }

                var url = FormsHelper.SaveFileAsync(item.formFile, "Returns").Result;
                if (url == null)
                {
                    res.Status = SubmissionStatus.Failed;
                    res.Messages.Add("Could not store file.");
                    results.Add(res);
                    continue;
                }

                var submission = new ReturnSubmission
                {
                    ExpectedReturnId = item.ExpectedReturnId,
                    SaccoId = SaccoId,
                    SubmittedAt = dto.SubmissionDate,
                    FileUrl = url,
                    //Status = SubmissionStatus.Draft
                };
                _context.ReturnSubmissions.Add(submission);
                _context.SaveChanges();
                res.SubmissionId = submission.Id;


                var parse = _excelParser.ParseAsync(item.FormFile, expected.ReturnForm.Code).Result;

                if (!parse.Success)
                {
                    submission.Status = SubmissionStatus.Failed;
                    submission.Messages = parse.Errors;
                    _context.SaveChanges();

                    res.Status = SubmissionStatus.Failed;
                    res.Messages = parse.Errors;
                    results.Add(res);
                    continue;
                }

                foreach (var row in parse.Rows)
                {
                    row.ReturnSubmissionId = submission.Id;
                    _context.Add(row.ToEntity());
                }
                _context.SaveChanges();
                res.Status = SubmissionStatus.Draft;
                res.Messages.Add("Saved as draft.");
                results.Add(res);
                

            }
            return Task.FromResult<IList<SubmissionResultDto>>(results);
        }
    }
}
