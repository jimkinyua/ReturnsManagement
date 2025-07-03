namespace Returns.DTOs.PeriodManagement
{
    public class FrequencyCatalogDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int IntervalDays { get; set; }
        public int DefaultDeadlineOffset { get; set; }
        public string LabelStrategy { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}