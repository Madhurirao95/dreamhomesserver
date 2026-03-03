using DREAMHOMES.Controllers.DTOs.AIGenerateDescription;
using DREAMHOMES.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DREAMHOMES.Controllers
{
    /// <summary>
    /// Controller for AI-powered property operations.
    /// Handles requests for generating property listing descriptions using Gemini API.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class AIController : ControllerBase
    {
        private readonly ILogger<AIController> _logger;
        private readonly IAIService _aiService;

        /// <summary>
        /// Initializes a new instance of the AIController class.
        /// </summary>
        /// <param name="logger">Logger for recording HTTP request/response events</param>
        /// <param name="aiService">Service for AI-powered description generation</param>
        public AIController(ILogger<AIController> logger, IAIService aiService)
        {
            _logger = logger;
            _aiService = aiService;
        }

        /// <summary>
        /// Generates a compelling property listing description based on property details.
        /// Delegates to AIService which handles Gemini API communication with retry logic.
        /// </summary>
        /// <param name="request">The property listing details DTO</param>
        /// <returns>
        /// 200 OK with generated description on success
        /// 500 Internal Server Error if API calls fail after retries or unexpected errors occur
        /// </returns>
        /// <remarks>
        /// POST /ai/generateDescription
        /// Request body example:
        /// {
        ///   "streetAddress": "123 Main St",
        ///   "city": "Springfield",
        ///   "state": "IL",
        ///   "zipCode": "62701",
        ///   "bedRooms": 3,
        ///   "bathRooms": 2,
        ///   ...
        /// }
        /// </remarks>
        [HttpPost("generateDescription")]
        public async Task<IActionResult> GenerateDescription([FromBody] GenerateListingDescriptionRequestDTO request)
        {
            try
            {
                // Delegate to AIService which handles Gemini API communication with retries
                var description = await _aiService.GenerateDescriptionAsync(request);
                return Ok(new { description });
            }
            catch (HttpRequestException ex)
            {
                // API communication error (exhausted retries)
                _logger.LogError(ex, "Error calling Gemini API");
                return StatusCode(500, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                // Unexpected errors (malformed response, parsing errors, etc.)
                _logger.LogError(ex, "Unexpected error generating description");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
