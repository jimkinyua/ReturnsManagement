using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Returns.DTOs.Forms;
using Returns.Helpers.Interfaces;
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

        public ReturnsReminderService( ReturnsDbContext db, IEmailService mail, ILogger<ReturnsReminderService> log, IComplianceService complianceService)
        {
            _context = db;
            _mail = mail;
            _log = log;
            this.complianceService = complianceService;
        }
    
        public  void SendRemindersAsync(CancellationToken ct)
        {

            var allSaccos = complianceService.GetAllSaccosAsync().Result;

            // 2. Send each one a polite nudge
            foreach (var sacco in allSaccos)
            {

              /*  var FormsForThisSacco = await _context.ReturnForms
                 .Where(x => x.SaccoTypeId == sacco.SaccoType)
                 .Include(f => f.Period)
                 .ToListAsync();

                foreach (var form in FormsForThisSacco)
                {
                    if (ReturnsHelper.IsFormDueForSubmission(form, DateTime.Now))
                    {
                        (DateTime reportingStartDate, DateTime reportingEndDate) = ReturnsHelper.GetReportingPeriod(form, DateTime.Now);

                        DateTime dueDate = ReturnsHelper.GetDueDate(form, reportingEndDate);
                        if (sacco.SaccoType == Constants.SaccoType.DepositTaking.ToString())
                        {
                            if (form.IsCapitalAdequencyForm)
                            {
                                var capitalAdequacyReturn = await _context.DTCapitalAdequacyReturns.Where(x => x.EndDate.Date == reportingEndDate.Date).FirstOrDefaultAsync();

                                if (capitalAdequacyReturn != null)
                                {
                                    continue; 
                                }
                            }
                        }
                        else
                        {

                        }
                     
                     
                    }
                }*/


                        var body = $"""
                            Dear {sacco.SaccoName} team,

                            Our records show we have not received your returns for {DateTime.Now.Month.ToString()}.
                            Please log into the portal and file them at your earliest convenience
                            to avoid penalties.

                            Thank you,
                            Compliance Desk
                            """;

                _mail.SendEmailAsync(
                   sacco.OfficialSaccoEmail,
                   $"Reminder: submit your  returns",
                   body.Replace("\n", "<br/>")
                   );

                _log.LogInformation("Reminder sent to {Sacco}", sacco.SaccoName);
            }

            _log.LogInformation("Returns-reminder run complete – {Count} email(s) sent.", allSaccos.Count);
        }
    }
}
