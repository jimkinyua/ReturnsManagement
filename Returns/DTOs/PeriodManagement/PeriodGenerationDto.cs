namespace Returns.DTOs.PeriodManagement
{
    public class PeriodGenerationRequestDto
    {
        public int YearId { get; set; }
        public List<int> FrequencyIds { get; set; } = new List<int>();
    }

    public class PeriodGenerationPreviewDto
    {
        public int YearId { get; set; }
        public int Year { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<FrequencyPreviewDto> Frequencies { get; set; } = new List<FrequencyPreviewDto>();
        public int TotalPeriodsToGenerate { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
    }

    public class FrequencyPreviewDto
    {
        public int FrequencyId { get; set; }
        public string FrequencyCode { get; set; } = null!;
        public string FrequencyName { get; set; } = null!;
        public List<PeriodPreviewDto> Periods { get; set; } = new List<PeriodPreviewDto>();
        public int ExistingPeriodsCount { get; set; }
        public int NewPeriodsCount { get; set; }
    }

    public class PeriodPreviewDto
    {
        public int SequenceNo { get; set; }
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime FilingDeadline { get; set; }
        public bool AlreadyExists { get; set; }
    }

    public class PeriodGenerationResultDto
    {
        public int YearId { get; set; }
        public int TotalPeriodsCreated { get; set; }
        public int TotalPeriodsSkipped { get; set; }
        public List<FrequencyResultDto> FrequencyResults { get; set; } = new List<FrequencyResultDto>();
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
    }

    public class FrequencyResultDto
    {
        public int FrequencyId { get; set; }
        public string FrequencyName { get; set; } = null!;
        public int PeriodsCreated { get; set; }
        public int PeriodsSkipped { get; set; }
        public List<string> CreatedPeriodNames { get; set; } = new List<string>();
    }
}