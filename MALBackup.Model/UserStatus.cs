using System.Text.Json.Serialization;

namespace MALBackup.Model
{
    /// <summary>
    /// The anime watching status by the user.
    /// </summary>
    public class UserStatus
    {
        /// <summary>
        /// The user watching status for the anime.
        /// </summary>
        [JsonPropertyName( "status" )]
        public Status Status { get; set; }
        /// <summary>
        /// Number of user watched episodes.
        /// </summary>
        [JsonPropertyName( "num_episodes_watched" )]
        public int WatchedEpisodes { get; set; }
        /// <summary>
        /// The number of times the user has reviewed the anime.
        /// </summary>
        [JsonPropertyName( "num_times_rewatched" )]
        public int RewatchedTimes { get; set; }
        /// <summary>
        /// The number of episodes seen by the user during his rewatching.
        /// </summary>
        [JsonPropertyName( "rewatch_value" )]
        public int RewatchedValue { get; set; }
    }
}
