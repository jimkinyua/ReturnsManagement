namespace Returns.DTOs.Returns.Returns_Submission.DT
{
    public class OtherReturnDTO :CommonFormDTO
    {
        public string FormName { get; set; }
        public string FileUrl { get; set; }
        public string SaccoId { get; set; }
        public string SaccoType { get; set; }
        public string SaccoName { get; set; }
        public string ReturnId { get; set; }
    }
}
