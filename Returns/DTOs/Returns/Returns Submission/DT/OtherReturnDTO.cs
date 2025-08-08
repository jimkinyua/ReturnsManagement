namespace Returns.DTOs.Returns.Returns_Submission.DT
{
    public class OtherReturnDTO :CommonFormDTO
    {
        public string FormName { get; set; } = string.Empty;
        public string SaccoId { get; set; } = string.Empty;
        public string SaccoType { get; set; } = string.Empty;
        public string SaccoName { get; set; } = string.Empty;
        public string ReturnId { get; set; } = string.Empty;
    }
}
