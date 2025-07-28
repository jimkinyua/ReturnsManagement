using Returns.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace Returns.Models
{
    public class SaccoReminderLog:CommonFields
    {
        public string SaccoId { get; set; }
        public DateTime LastReminderSent { get; set; } // When the last reminder was sent

        public int RemindersSentCount { get; set; } // Total reminders sent for this SACCO (for escalation)

        [StringLength(50)]
        public string? ReminderType { get; set; } // E.g., "7-day", "1-day", "Overdue"

        [StringLength(50)]
        public string? LastReminderStatus { get; set; } // E.g., "Sent", "Failed", "Skipped"

        public string? PendingReturns { get; set; } // JSON or comma-separated list of pending return IDs or form names

        public DateTime? NextReminderDate { get; set; } // When the next reminder is allowed (for throttling)

        [StringLength(50)]
        public string? EscalationLevel { get; set; } // E.g., "None", "SecondaryEmail", "Manual"

        public int? PreferredReminderFrequencyDays { get; set; } // SACCO-specific override for reminder interval (e.g., 7 days)

        public DateTime? UpdatedAt { get; set; } // When this log entry was last updated

        [StringLength(50)]
        public string? SentBy { get; set; } // E.g., "System", or admin username if manual
    }
}
