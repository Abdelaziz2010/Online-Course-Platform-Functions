using System.Text.Json.Serialization;

namespace EduPlatform.Functions.DTOs
{
    public class VideoRequestDTO 
    {
        [JsonPropertyName("videoRequestId")]
        public int VideoRequestId { get; set; }
    }
}
