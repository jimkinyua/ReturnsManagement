namespace Returns.DTOs.Returns.Returns_Submission.NWDT
{
    public class SectoralLendingDTO
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ICollection<SectoralLendingDataDTO> SubSectorData { get; set; } = new List<SectoralLendingDataDTO>();

    }
}
