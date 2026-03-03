using System.Text.Json.Serialization;

namespace DREAMHOMES.Controllers.DTOs.AIGenerateDescription.Gemini
{
    public class GeminiResponseDTO
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidateDTO> Candidates { get; set; } = new();
    }
}
