namespace MALBackup.Analytics
{
    public struct RemoveAnimeDiff : IAnimeDiff
    {
        public DateTime From { get; }

        public DateTime To { get; }

        public int AnimeId { get; }

        public RemoveAnimeDiff( DateTime from, DateTime to, int animeId )
        {
            if( from > to ) throw new ArgumentException( "from can't be more recent that to.", nameof( from ) );
            From = from;
            To = to;
            AnimeId = animeId;
        }

        public IEnumerable<Anime> Apply( IEnumerable<Anime> animes )
        {
            if( animes is null ) throw new ArgumentNullException( nameof( animes ) );

            int animeId = AnimeId;

            if( animes.FirstOrDefault( anime => anime.Id == animeId ) is null )
            {
                throw new ArgumentException( $"{nameof( animes )} doesn't contain any anime with this Id: {animeId}", nameof( animes ) );
            }
            return animes.Where( anime => anime.Id != animeId );
        }

        public void Accept( IDiffVisitor visitor ) => visitor.ApplyToDiff( this );
    }
}
