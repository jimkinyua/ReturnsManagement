using Returns.Models.Common;

namespace Returns.Models
{
    public class ReturnCompleteness:CommonFields
    {
        public string PeriodId { get; set; } = null!;
        public string SaccoId { get; set; } = null!;
        public Boolean IsComplete { get; set; } = false;
        public int ExpectedReturnCount { get; set; } = 0;
        public int ActualReturnCount { get; set; } = 0;
        public DateTime? LastUpdated { get; set; } = null;
        public string? MissingForms { get; set; } = null;

    }
}
