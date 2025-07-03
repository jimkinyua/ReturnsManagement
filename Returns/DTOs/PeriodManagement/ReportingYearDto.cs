namespace Returns.DTOs.PeriodManagement
{
    public class ReportingYearDto
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = null!;
        public int PeriodCount { get; set; } // Count of periods in this year
    }
}