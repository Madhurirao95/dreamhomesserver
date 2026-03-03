using DREAMHOMES.Controllers.DTOs.AIGenerateDescription;
using DREAMHOMES.Controllers.DTOs.AIGenerateDescription.Gemini;
using DREAMHOMES.Services.Interfaces;

namespace DREAMHOMES.Services
{
    /// <summary>
    /// Service for generating property listing descriptions using Google Gemini AI.
    /// Implements resilience patterns including exponential backoff retry logic and token optimization.
    /// </summary>
    public class AIService : IAIService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<AIService> _logger;
        
        // Gemini API configuration constants
        private const string Model = "gemini-2.5-flash";
        private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models";
        
        // Retry and optimization constants
        private const int MaxRetries = 3;
        private const int PropertiesMaxLength = 10000;

        /// <summary>
        /// Initializes a new instance of the AIService class.
        /// </summary>
        /// <param name="httpClientFactory">Factory for creating HTTP clients</param>
        /// <param name="config">Application configuration for API keys</param>
        /// <param name="logger">Logger for recording retry attempts and errors</param>
        public AIService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<AIService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Generates a compelling property listing description using the Gemini API.
        /// Implements exponential backoff retry logic for transient failures (5xx, 429 errors, timeouts).
        /// </summary>
        /// <param name="request">The property listing details</param>
        /// <returns>AI-generated property description</returns>
        /// <exception cref="HttpRequestException">Thrown after max retries are exhausted</exception>
        /// <exception cref="InvalidOperationException">Thrown when API response is malformed</exception>
        public async Task<string> GenerateDescriptionAsync(GenerateListingDescriptionRequestDTO request)
        {
            var apiKey = _config["Gemini:ApiKey"];
            var url = $"{GeminiApiUrl}/{Model}:generateContent?key={apiKey}";
            
            // Build optimized prompt excluding unnecessary fields to reduce token usage
            var prompt = BuildPrompt(request);
            var geminiRequest = new GeminiRequestDTO
            {
                Contents =
                [
                    new GeminiContentDTO
                    {
                        Parts =
                        [
                            new GeminiPartDTO { Text = prompt }
                        ]
                    }
                ]
            };

            var client = _httpClientFactory.CreateClient();
            
            HttpResponseMessage? response = null;
            Exception? lastException = null;

            // Retry loop with exponential backoff: 1s (2^0), 2s (2^1), 4s (2^2)
            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                try
                {
                    response = await client.PostAsJsonAsync(url, geminiRequest);

                    if (response.IsSuccessStatusCode)
                    {
                        // Parse successful response from Gemini API
                        var geminiResponse = await response.Content.ReadFromJsonAsync<GeminiResponseDTO>();
                        return geminiResponse?.Candidates[0].Content.Parts[0].Text ?? throw new InvalidOperationException("Empty response from Gemini API");
                    }

                    // Retry on server errors (5xx) or rate limiting (429)
                    if (((int)response.StatusCode >= 500 || response.StatusCode == System.Net.HttpStatusCode.TooManyRequests) && attempt < MaxRetries - 1)
                    {
                        var delayMs = (int)Math.Pow(2, attempt) * 1000;
                        _logger.LogWarning($"Gemini API returned {response.StatusCode}. Retrying in {delayMs}ms (Attempt {attempt + 1}/{MaxRetries})");
                        await Task.Delay(delayMs);
                        continue;
                    }

                    // Non-retryable error - throw immediately
                    var error = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Gemini API error: {response.StatusCode} - {error}");
                }
                catch (HttpRequestException ex) when (attempt < MaxRetries - 1 && (ex.InnerException is TimeoutException or HttpIOException))
                {
                    // Retry on network-related exceptions (timeout, connection errors)
                    lastException = ex;
                    var delayMs = (int)Math.Pow(2, attempt) * 1000;
                    _logger.LogWarning(ex, $"Request timeout/failure. Retrying in {delayMs}ms (Attempt {attempt + 1}/{MaxRetries})");
                    await Task.Delay(delayMs);
                    continue;
                }
                catch (Exception ex)
                {
                    // Non-retryable exception - throw immediately
                    throw;
                }
            }

            // All retries exhausted - throw error
            if (response != null && !response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Gemini API error after {MaxRetries} retries: {response.StatusCode} - {error}");
            }

            throw lastException ?? new HttpRequestException($"Failed to get response from Gemini API after {MaxRetries} retries");
        }

        /// <summary>
        /// Builds an optimized prompt for the Gemini API to generate property descriptions.
        /// Excludes unnecessary fields to reduce token usage and costs.
        /// Truncates the Properties field to prevent excessive token consumption.
        /// </summary>
        /// <param name="request">The property listing details</param>
        /// <returns>Formatted prompt string for Gemini API</returns>
        private string BuildPrompt(GenerateListingDescriptionRequestDTO request)
        {
            // Truncate additional properties to prevent token overflow
            var truncatedProperties = request.Properties?.Length > PropertiesMaxLength
                ? request.Properties.Substring(0, PropertiesMaxLength) + "..."
                : request.Properties ?? string.Empty;

            // Optimized prompt with only essential property details
            // Excluded: ZipCode, ListingPrice, AmountPerSqFt, Unit (not needed for compelling description)
            // Included: BuildingType, LotArea, LotAreaUnit (necessary for complete property context)
            return $"""
You are an expert real estate copywriter. Write a compelling property listing description.

Location: {request.StreetAddress}, {request.City}, {request.State}
PropertyType: {request.Type} | BuildingType: {request.BuildingType}
Beds: {request.BedRooms} | Baths: {request.BathRooms} | Size: {request.Area} sqft
LotArea: {request.LotArea} {request.LotAreaUnit}
Built: {request.YearBuilt}

Features:
- Garage: {(request.HasGarage ? $"{request.NumberOfGarageSpace} spaces" : "No")}
- Pool: {(request.HasPool ? "Yes" : "No")}
- Fireplace: {(request.HasFirePlace ? $"{request.NumberOfFirePlace} fireplace(s)" : "No")}
- HOA: ${request.HOA}
{(string.IsNullOrEmpty(truncatedProperties) ? "" : $"- Other features: {truncatedProperties}")}

Write a compelling 2000 character max description. Professional tone. Highlight best features. End with "Book your viewing today!" Return only the description text.
""";
        }
    }
}
