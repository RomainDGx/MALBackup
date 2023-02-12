using MALBackup.Model;
using System.Text.Json.Serialization;

namespace MalBackup.FormatData
{
    internal class OldAnimeFormat
    {
        [JsonPropertyName( "anime_id" )]
        public int Id { get; set; }
        [JsonPropertyName( "anime_title" )]
        public string Title { get; set; } = string.Empty;
        [JsonPropertyName( "anime_num_episodes" )]
        public int NumEpisodes { get; set; }
        #region ListStatus
        [JsonPropertyName( "status" )]
        public Status Status { get; set; }
        [JsonPropertyName( "num_watched_episodes" )]
        public int NumWatchedEpisodes { get; set; }
        #endregion

        internal Anime ToAnime()
        {
            return new Anime
            {
                Id = Id,
                Title = Title,
                Episodes = NumEpisodes,
                UserStatus = new UserStatus
                {
                    Status = Status,
                    WatchedEpisodes = NumWatchedEpisodes,
                    RewatchedTimes = 0,
                    RewatchedValue = 0,
                }
            };
        }
    }
}
