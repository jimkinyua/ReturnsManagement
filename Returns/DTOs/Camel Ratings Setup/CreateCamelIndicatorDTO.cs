using System.ComponentModel.DataAnnotations;

public class CreateCamelIndicatorDTO
{
    [Required]
    public string CategoryId { get; set; } = null!;
    [Required]
    public string Name { get; set; } = null!;
    [Required]
    public decimal Weight { get; set; }
    [Required]
    public bool BetterHigher { get; set; }
}