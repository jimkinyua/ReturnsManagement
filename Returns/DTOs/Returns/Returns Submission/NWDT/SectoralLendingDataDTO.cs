namespace Returns.DTOs.Returns.Returns_Submission.NWDT
{
    public class SectoralLendingDataDTO
    {
        public decimal Amount { get; set; }
        public string Category { get; set; } = null!;
        public string SubCategory { get; set; } = null!;
        public string EconomicSectorName { get; set; } = null!;
    }
}
