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
        private readonly IConfiguration _config;



        public ReturnsReminderService(ReturnsDbContext db, IConfiguration configuration,  IEmailService mail, ILogger<ReturnsReminderService> log, IComplianceService complianceService, ILogger<ReturnsReminderService> logger)
        {
            _context = db;
            _emailService = mail;
            _log = log;
            _complianceService = complianceService;
            _formProcessor = new FormProcessingService(_context, _log);
            _logger = logger;
            _config = configuration;
        }

        public async Task SendRemindersAsync(CancellationToken cancellationToken)
        {
/*            try
            {
                var now = DateTime.UtcNow;

                var saccos = await _complianceService.GetAllSaccosAsync();

                var minDaysBetweenOverdueReminders = 3; // From config
                var escalationDaysLate = 30; // From config
                var portalLoginUrl = _config.GetValue<string>("Urls:SaccoUrl");

                foreach (var sacco in saccos)
                {
                    if (string.IsNullOrEmpty(sacco.OfficialSaccoEmail) || !IsValidEmail(sacco.OfficialSaccoEmail))
                    {
                        _logger.LogWarning("Sacco {SaccoId} has invalid or no email: {Email}", sacco.Id, sacco.OfficialSaccoEmail);
                        continue;
                    }

                    var lastReminder = await _context.SaccoReminderLogs.FirstOrDefaultAsync(l => l.SaccoId == sacco.Id, cancellationToken);

                    var expectedReturns = await _context.ExpectedReturns
                        .Include(er => er.Period)
                        .ThenInclude(p => p.FrequencyCatalog)
                        .Include(er => er.ReturnForm)
                        .Include(er => er.ReturnSubmissions.Where(rs => rs.SaccoId == sacco.Id && rs.Status == ExpectedStatus.Filed.ToString()))
                        .Where(er => er.IsActive && er.ReturnForm.SaccoTypeId == sacco.SaccoType)
                        .ToListAsync(cancellationToken);

                    var pendingReturns = expectedReturns.Where(er => !er.ReturnSubmissions.Any()).ToList();

                    if (!pendingReturns.Any())
                    {
                        continue;
                    }

                    var reminders = new List<string>();
                    var reminderTypes = new List<string>();
                    bool hasOverdue = false;

                    var returnsByFrequency = pendingReturns.GroupBy(er => new { Frequency = er.Period.FrequencyCatalog.Name, Period = er.Period.Name });

                    foreach (var frequencyGroup in returnsByFrequency)
                    {
                        var frequency = frequencyGroup.Key.Frequency;
                        var periodName = frequencyGroup.Key.Period;
                        var deadline = frequencyGroup.First().FilingDeadline;

                        var daysToDeadline = (deadline - now).TotalDays;

                        if (daysToDeadline <= 0) hasOverdue = true;

                        var formNames = frequencyGroup.Select(er => er.ReturnForm.FormName).ToList();
                        var formsList = string.Join(", ", formNames);

                        string reminderMessage = null;
                        string reminderType = null;

                        if (daysToDeadline <= 7 && daysToDeadline > 1)
                        {
                            reminderMessage = $"Your {frequency} returns for {periodName} ({formsList}) are due in {Math.Floor(daysToDeadline)} days on {deadline:yyyy-MM-dd}.";
                            reminderType = "7-day";
                        }
                        else if (daysToDeadline <= 1 && daysToDeadline > 0)
                        {
                            reminderMessage = $"Urgent: Your {frequency} returns for {periodName} ({formsList}) are due tomorrow on {deadline:yyyy-MM-dd}.";
                            reminderType = "1-day";
                        }
                        else if (daysToDeadline <= 0)
                        {
                            var daysLate = Math.Abs(Math.Floor(daysToDeadline));
                            if (daysLate >= escalationDaysLate)
                            {
                                _logger.LogWarning("Sacco {SaccoId} has returns over {Days} days late for {Period}. Escalate manually.", sacco.Id, escalationDaysLate, periodName);
                                continue;
                            }
                            reminderMessage = $"Overdue: Your {frequency} returns for {periodName} ({formsList}) are {daysLate} day{(daysLate > 1 ? "s" : "")} late (deadline was {deadline:yyyy-MM-dd}). Please submit immediately.";
                            reminderType = "Overdue";
                        }

                        if (!string.IsNullOrEmpty(reminderMessage))
                        {
                            reminders.Add(reminderMessage);
                            reminderTypes.Add(reminderType);
                        }
                    }

                    if (!reminders.Any())
                    {
                        continue;
                    }
                    // Throttling check
                    var effectiveFrequencyDays = lastReminder?.PreferredReminderFrequencyDays ?? minDaysBetweenOverdueReminders;
                    if (hasOverdue && lastReminder != null && lastReminder.NextReminderDate > now)
                    {
                        _logger.LogInformation("Skipping reminder for Sacco {SaccoId} due to throttling (next allowed: {NextDate}).", sacco.Id, lastReminder.NextReminderDate);
                        continue;
                    }

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

                    body.AppendLine($"Please log in to submit: {portalLoginUrl}");
                    body.AppendLine();
                    body.AppendLine("Best regards,");
                    body.AppendLine("SASRA Team");

                    string sendStatus;
                    try
                    {
                        await _emailService.SendEmailAsync(sacco.OfficialSaccoEmail, subject, body.ToString());
                        sendStatus = "Sent";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send reminder to Sacco {SaccoId}", sacco.Id);
                        sendStatus = "Failed";
                    }

                    // Update or create reminder log
                    var pendingReturnsList = string.Join(",", pendingReturns.Select(er => er.ReturnForm.FormName));
                    if (lastReminder == null)
                    {
                        lastReminder = new SaccoReminderLog
                        {
                            SaccoId = sacco.Id,
                            LastReminderSent = now,
                            RemindersSentCount = 1,
                            ReminderType = string.Join(",", reminderTypes),
                            LastReminderStatus = sendStatus,
                            PendingReturns = pendingReturnsList,
                            NextReminderDate = hasOverdue ? now.AddDays(effectiveFrequencyDays) : null,
                            EscalationLevel = "None",
                            PreferredReminderFrequencyDays = null, // SACCO can set later
                            CreatedAt = now,
                            SentBy = "System"
                        };
                        _context.SaccoReminderLogs.Add(lastReminder);
                    }
                    else
                    {
                        lastReminder.LastReminderSent = now;
                        lastReminder.RemindersSentCount++;
                        lastReminder.ReminderType = string.Join(",", reminderTypes);
                        lastReminder.LastReminderStatus = sendStatus;
                        lastReminder.PendingReturns = pendingReturnsList;
                        lastReminder.NextReminderDate = hasOverdue ? now.AddDays(effectiveFrequencyDays) : null;
                        lastReminder.EscalationLevel = lastReminder.RemindersSentCount >= 5 ? "SecondaryEmail" : "None"; // Example escalation
                        lastReminder.UpdatedAt = now;
                        lastReminder.SentBy = "System";
                    }
                    await _context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Sent reminder email to Sacco {SaccoId} for {ReminderCount} items. Status: {Status}", sacco.Id, reminders.Count, sendStatus);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending returns reminders");
            }
*/        }

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
