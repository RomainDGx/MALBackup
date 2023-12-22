using System.Text.Json.Serialization;

namespace MALBackup.App
{
    public class ErrorResponse
    {
        [JsonPropertyName( "error" )]
        public string Error { get; set; } = string.Empty;

        [JsonPropertyName( "message" )]
        public string? Message { get; set; }

        public override string ToString() => $"{{ error: {Error}, message: {Message} }}";
    }
}
