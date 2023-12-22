using MALBackup.Model;
using System.Text.Json.Serialization;

namespace MALBackup.App
{
    internal class AnimeListData
    {
        [JsonPropertyName( "node" )]
        public Anime? Anime { get; set; }

        [JsonPropertyName( "list_status" )]
        public UserStatus? UserListStatus { get; set; }
    }
}
