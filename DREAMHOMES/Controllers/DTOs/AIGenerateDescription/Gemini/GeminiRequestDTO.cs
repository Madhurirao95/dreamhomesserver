using System.Text.Json.Serialization;

namespace DREAMHOMES.Controllers.DTOs.AIGenerateDescription.Gemini
{
    public class GeminiRequestDTO
    {
        [JsonPropertyName("contents")]
        public List<GeminiContentDTO> Contents { get; set; } = new();
    }
}
