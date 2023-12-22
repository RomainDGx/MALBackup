using System.Text.Json.Serialization;

namespace MALBackup.Model
{
    /// <summary>
    /// Class that define basic anime data and the user watching status.
    /// </summary>
    public class Anime
    {
        /// <summary>
        /// The identifier of the anime (in MyAnimeList).
        /// </summary>
        [JsonPropertyName( "id" )]
        public int Id { get; set; }
        /// <summary>
        /// The title of the anime.
        /// </summary>
        [JsonPropertyName( "title" )]
        public string? Title { get; set; }
        /// <summary>
        /// The number of episodes of the anime.
        /// If the value is 0, the number of episodes is not defined (in MyAnimeList).
        /// </summary>
        [JsonPropertyName( "num_episodes" )]
        public int Episodes { get; set; }
        /// <summary>
        /// The anime watching status by the user.
        /// </summary>
        [JsonPropertyName( "my_list_status" )]
        public UserStatus? UserStatus { get; set; }
    }
}
