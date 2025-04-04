using System.ComponentModel.DataAnnotations;

namespace Returns.DTOs
{
    public class CameLIndicatorDTO
    {
        public string CategoryId { get; set; } = null!;
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal Weight { get; set; }
        public Boolean BetterHigher { get; set; }
    }
}
