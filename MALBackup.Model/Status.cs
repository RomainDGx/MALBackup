namespace MALBackup.Model
{
    /// <summary>
    /// The watching status of an anime for a user.
    /// The value are based from the MyAnimeList internal data.
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// The user is watching the anime.
        /// </summary>
        Watching = 1,
        /// <summary>
        /// The user has finished to watch the anime.
        /// </summary>
        Completed = 2,
        /// <summary>
        /// The user has paused the anime.
        /// </summary>
        OnHold = 3,
        /// <summary>
        /// The user has abandoned the watching of the anime.
        /// </summary>
        Dropped = 5,
        /// <summary>
        /// The user want to watch the anime later.
        /// </summary>
        PlanToWatch = 6,
    }
}
