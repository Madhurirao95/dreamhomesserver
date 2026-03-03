using System.Text.Json.Serialization;

namespace DREAMHOMES.Controllers.DTOs.AIGenerateDescription.Gemini
{
    public class GeminiPartDTO
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}
