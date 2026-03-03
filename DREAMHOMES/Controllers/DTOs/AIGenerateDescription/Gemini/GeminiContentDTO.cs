using System.Text.Json.Serialization;

namespace DREAMHOMES.Controllers.DTOs.AIGenerateDescription.Gemini
{
    public class GeminiContentDTO
    {
        [JsonPropertyName("parts")]
        public List<GeminiPartDTO> Parts { get; set; } = new();
    }
}
