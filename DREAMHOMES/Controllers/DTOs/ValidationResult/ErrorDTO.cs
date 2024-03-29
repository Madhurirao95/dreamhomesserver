namespace DREAMHOMES.Controllers.DTOs.ValidationResult
{
    /// <summary>
    /// Represents the structure of errors.
    /// </summary>
    public class ErrorDTO
    {
        public string Message { get; set; } = string.Empty;

        public IEnumerable<string>? MessageArguments { get; set; }
    }
}
