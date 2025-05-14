using DocumentFormat.OpenXml.InkML;
using iTextSharp.text.log;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Returns.DTOs.Forms;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;
using System;
using System.Reflection.Metadata;

namespace Returns.Helpers.Reminders
{
    public class ReturnsReminderService
    {
        private readonly ReturnsDbContext _context;
        private readonly IEmailService _mail;
        private readonly ILogger<ReturnsReminderService> _log;
        private readonly IComplianceService complianceService;
        private readonly FormProcessingService _formProcessor;


        public ReturnsReminderService( ReturnsDbContext db, IEmailService mail, ILogger<ReturnsReminderService> log, IComplianceService complianceService)
        {
            _context = db;
            _mail = mail;
            _log = log;
            this.complianceService = complianceService;
            _formProcessor = new FormProcessingService(_context, _log);
        }

        public async Task SendRemindersAsync(CancellationToken ct)
        {
            ReturnsHelper returnsHelper = new ReturnsHelper(_context);

            var allSaccos =  await complianceService.GetAllSaccosAsync();

            // 2. Send each one a polite nudge
            foreach (var sacco in allSaccos)
            {

                var FormsForThisSacco = await _context.ReturnForms
                 .Where(x => x.SaccoTypeId == sacco.SaccoType)
                 .Include(f => f.Period)
                 .ToListAsync();

                foreach (var form in FormsForThisSacco)
                {
                    if (returnsHelper.IsFormDueForSubmission(form, DateTime.Now))
                    {
                        (DateTime reportingStartDate, DateTime reportingEndDate) = returnsHelper.GetReportingPeriod(form, DateTime.Now);

                       var Result = await _formProcessor.IsThereAnyExistingReturn(form, reportingEndDate, sacco.SaccoType, sacco.Id);
                        if (!Result.ReturnExists)
                        {
                            var body = $"""
                            Dear {sacco.SaccoName} Team,

                            Our records show we have not yet received your {form.FormName} return for the period {reportingStartDate:dd/MM/yyyy} – {reportingEndDate:dd/MM/yyyy}.  
                            Please log in to the portal and submit the return at your earliest convenience to avoid penalties.

                            Thank you for your prompt attention.

                            Compliance Desk
                            """;

                            await _mail.SendEmailAsync(
                               sacco.OfficialSaccoEmail,
                               $"Reminder: submit your  returns",
                               body.Replace("\n", "<br/>")
                               );

                            _log.LogInformation("Reminder sent to {Sacco}", sacco.SaccoName);
                        }

                    }
                }


      
            }

            _log.LogInformation("Returns-reminder run complete – {Count} email(s) sent.", allSaccos.Count);
        }
    }
}
