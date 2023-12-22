using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MALBackup.App
{
    internal class ResponsePayload
    {
        [JsonPropertyName( "data" )]
        public List<AnimeListData>? Data { get; set; }

        [JsonPropertyName( "paging" )]
        public PagingData? Paging { get; set; }
    }
}
