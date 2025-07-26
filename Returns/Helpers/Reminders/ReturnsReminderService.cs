using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Returns.DTOs.Forms;
using Returns.Helpers.Enums;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;
using System;
using System.Reflection.Metadata;
using System.Text;

namespace Returns.Helpers.Reminders
{
    public class ReturnsReminderService
    {
        private readonly ReturnsDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<ReturnsReminderService> _log;
        private readonly IComplianceService _complianceService;
        private readonly FormProcessingService _formProcessor;
        private readonly ILogger<ReturnsReminderService> _logger;


        public ReturnsReminderService(ReturnsDbContext db, IEmailService mail, ILogger<ReturnsReminderService> log, IComplianceService complianceService, ILogger<ReturnsReminderService> logger)
        {
            _context = db;
            _emailService = mail;
            _log = log;
            _complianceService = complianceService;
            _formProcessor = new FormProcessingService(_context, _log);
            _logger = logger;
        }

        public async Task SendRemindersAsync(CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTime.UtcNow;

                var saccos = await _complianceService.GetAllSaccosAsync();

                foreach (var sacco in saccos)
                {
                    if (string.IsNullOrEmpty(sacco.OfficialSaccoEmail))
                    {
                        _logger.LogWarning("Sacco {SaccoId} has no email for reminders.", sacco.Id);
                        continue;
                    }

                    if(!IsValidEmail(sacco.OfficialSaccoEmail))
                    {
                        _logger.LogWarning("Sacco {SaccoId} has invalid email: {Email}", sacco.Id, sacco.OfficialSaccoEmail);
                        continue;
                    }

                    // Fetch pending expected returns for this Sacco's type (not filed)
                    var expectedReturns = await _context.ExpectedReturns
                        .Include(er => er.Period)
                        .ThenInclude(p => p.FrequencyCatalog)
                        .Include(er => er.ReturnForm)
                        .Include(er => er.ReturnSubmissions.Where(rs => rs.SaccoId == sacco.Id && rs.Status == ExpectedStatus.Filed.ToString()))  // Check per Sacco
                        .Where(er => er.IsActive && er.ReturnForm.SaccoTypeId == sacco.SaccoType)
                        .ToListAsync(cancellationToken);

                    var pendingReturns = expectedReturns.Where(er => !er.ReturnSubmissions.Any()).ToList();

                    if (!pendingReturns.Any()) continue;

                    var reminders = new List<string>();  // Collect messages for this SACCO

                    // Group returns by Frequency/Period for batching
                    var returnsByFrequency = pendingReturns.GroupBy(er => new { Frequency = er.Period.FrequencyCatalog.Name, Period = er.Period.Name });

                    foreach (var frequencyGroup in returnsByFrequency)
                    {
                        var frequency = frequencyGroup.Key.Frequency;
                        var periodName = frequencyGroup.Key.Period;  // e.g., "Q1 2025"
                        var deadline = frequencyGroup.First().FilingDeadline;

                        var daysToDeadline = (deadline - now).TotalDays;

                        // Collect form names for this group
                        var formNames = frequencyGroup.Select(er => er.ReturnForm.FormName).ToList();
                        var formsList = string.Join(", ", formNames);

                        string reminderMessage = null!;

                        if (daysToDeadline <= 7 && daysToDeadline > 1)
                        {
                            // 1 week before
                            reminderMessage = $"Your {frequency} returns for {periodName} ({formsList}) are due in {Math.Floor(daysToDeadline)} days on {deadline:yyyy-MM-dd}.";
                        }
                        else if (daysToDeadline <= 1 && daysToDeadline > 0)
                        {
                            // 1 day before
                            reminderMessage = $"Urgent: Your {frequency} returns for {periodName} ({formsList}) are due tomorrow on {deadline:yyyy-MM-dd}.";
                        }
                        else if (daysToDeadline <= 0)
                        {
                            // Late: remind every day
                            var daysLate = Math.Abs(Math.Floor(daysToDeadline));
                            reminderMessage = $"Overdue: Your {frequency} returns for {periodName} ({formsList}) are {daysLate} day{(daysLate > 1 ? "s" : "")} late (deadline was {deadline:yyyy-MM-dd}). Please submit immediately.";
                        }

                        if (!string.IsNullOrEmpty(reminderMessage))
                        {
                            reminders.Add(reminderMessage);
                        }
                    }

                    if (reminders.Any())
                    {
                        // Build email body
                        var subject = "SASRA Returns Submission Reminder";
                        var body = new StringBuilder();
                        body.AppendLine("Dear SACCO Administrator,");
                        body.AppendLine();
                        body.AppendLine("This is a reminder regarding your pending returns submissions:");
                        body.AppendLine();

                        foreach (var reminder in reminders)
                        {
                            body.AppendLine($"- {reminder}");
                            body.AppendLine();
                        }

                        body.AppendLine("Please log in to the system to submit these returns promptly.");
                        body.AppendLine();
                        body.AppendLine("Best regards,");
                        body.AppendLine("SASRA Team");

                        // Send email
                        await _emailService.SendEmailAsync(sacco.OfficialSaccoEmail, subject, body.ToString());

                        _logger.LogInformation("Sent reminder email to Sacco {SaccoId} for {ReminderCount} items.", sacco.Id, reminders.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending returns reminders");
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
