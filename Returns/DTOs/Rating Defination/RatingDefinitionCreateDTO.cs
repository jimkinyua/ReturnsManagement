namespace Returns.DTOs.Rating_Defination
{
    public class RatingDefinitionCreateDTO
    {
        public string RatingName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string SaccoType { get; set; } = null!;
        public List<string> FormCodes { get; set; } = new List<string>();
    }
}
