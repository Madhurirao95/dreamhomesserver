using System.Text.Json.Serialization;

namespace DREAMHOMES.Controllers.DTOs.AIGenerateDescription.Gemini
{
    public class GeminiCandidateDTO
    {
        [JsonPropertyName("content")]
        public GeminiContentDTO Content { get; set; } = new();
    }
}
