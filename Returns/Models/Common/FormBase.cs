namespace Returns.Models.Common
{
    public class FormBase : CommonFields
    {
        public bool IsAmended { get; set; } = false;
        public string? PreviousReturnId { get; set; }
    }
}
