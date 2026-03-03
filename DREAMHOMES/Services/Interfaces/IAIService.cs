using DREAMHOMES.Controllers.DTOs.AIGenerateDescription;

namespace DREAMHOMES.Services.Interfaces
{
    /// <summary>
    /// Service interface for AI-powered property listing description generation.
    /// Uses Google Gemini API to generate compelling real estate descriptions.
    /// </summary>
    public interface IAIService
    {
        /// <summary>
        /// Generates a compelling property listing description using AI.
        /// </summary>
        /// <param name="request">The property details DTO containing listing information</param>
        /// <returns>A generated property description string</returns>
        /// <exception cref="HttpRequestException">Thrown when API calls fail after retries</exception>
        /// <exception cref="InvalidOperationException">Thrown when API returns empty response</exception>
        Task<string> GenerateDescriptionAsync(GenerateListingDescriptionRequestDTO request);
    }
}
