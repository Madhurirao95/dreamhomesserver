namespace SPOTAHOME.Controllers.DTOs.ValidationResult
{
    /// <summary>
    /// Represents the structure of Validation Result.
    /// </summary>
    public class ValidationResultDTO
    {
        public IEnumerable<ErrorDTO>? Errors { get; set; }
    }
}
