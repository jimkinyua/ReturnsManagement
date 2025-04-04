namespace Returns.DTOs.Compliance
{
    public class OperationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public static OperationResult Ok() => new OperationResult { Success = true };
        public static OperationResult Fail(string errorMessage) => new OperationResult { Success = false, ErrorMessage = errorMessage };
    }
}
