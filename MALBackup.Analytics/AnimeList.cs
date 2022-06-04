namespace MALBackup.Analytics
{
    /// <summary cref="Anime">
    /// Inherance from <see cref="List{T}"/>. Is identified by <see cref="At"/> property.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AnimeList : List<Anime>
    {
        /// <summary>
        /// The date of the anime list.
        /// </summary>
        public DateTime At { get; }

        /// <param name="at">The date of the anime list.</param>
        public AnimeList( DateTime at ) : this( at, Array.Empty<Anime>() )
        {
        }

        /// <param name="at">The date of the anime list.</param>
        /// <param name="animes">An enumerable of <see cref="Anime"/>.</param>
        public AnimeList( DateTime at, IEnumerable<Anime> animes ) : base( animes )
        {
            At = at;
        }

        /// <remarks>
        /// <para>Compare current <see cref="AnimeList"/> from other <see cref="AnimeList"/>.</para>
        /// <para>Throws an <see cref="ArgumentException"/> if <paramref name="other"/> is older that current <see cref="AnimeList"/>.</para>
        /// </remarks>
        /// <param name="other">An <see cref="AnimeList"/> to compare with the current <see cref="AnimeList"/>.</param>
        /// <returns>List of differences between the two <see cref="AnimeList"/>.</returns>
        /// <exception cref="ArgumentException"></exception>
        public IEnumerable<IAnimeDiff> Compare( AnimeList other )
        {
            if( other.At < At ) throw new ArgumentException( "The other anime list should be more recent." );

            List<IAnimeDiff> diffs = new();

            foreach( Anime otherAnime in other )
            {
                Anime? thisAnime = this.FirstOrDefault( a => a.Id == otherAnime.Id );

                if( thisAnime is null )
                {
                    diffs.Add( new AddAnimeDiff( At, other.At, otherAnime ) );
                    continue;
                }
                if( thisAnime.WatchedEpisodes != otherAnime.WatchedEpisodes )
                {
                    diffs.Add( new WatchedEpisodesDiff( At, other.At, otherAnime.Id, otherAnime.WatchedEpisodes - thisAnime.WatchedEpisodes ) );
                }
                if( thisAnime.Status != otherAnime.Status )
                {
                    diffs.Add( new StatusDiff( At, other.At, otherAnime.Id, otherAnime.Status ) );
                }
            }
            foreach( Anime thisAnime in this )
            {
                Anime? otherAnime = other.FirstOrDefault( i => i.Id == thisAnime.Id );

                if( otherAnime is null )
                {
                    diffs.Add( new RemoveAnimeDiff( At, other.At, thisAnime.Id ) );
                }
            }

            return diffs;
        }
    }
}
