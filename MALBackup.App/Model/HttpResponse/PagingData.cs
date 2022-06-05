using System.Text.Json.Serialization;

namespace MALBackup.App
{
    internal class PagingData
    {
        [JsonPropertyName( "previous" )]
        public string? Previous { get; set; }

        [JsonPropertyName( "next" )]
        public string? Next { get; set; }
    }
}
