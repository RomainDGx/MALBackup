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
        public int Id { get; set; }
        /// <summary>
        /// The title of the anime.
        /// </summary>
        public string? Title { get; set; }
        /// <summary>
        /// The number of episodes of the anime.
        /// If the value is 0, the number of episodes is not defined (in MyAnimeList).
        /// </summary>
        public int Episodes { get; set; }
        /// <summary>
        /// Number of user watched episodes.
        /// </summary>
        public int WatchedEpisodes { get; set; }
        /// <summary>
        /// The user watching status for the anime.
        /// </summary>
        public Status Status { get; set; }
    }
}
