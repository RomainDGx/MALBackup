namespace MALBackup.Analytics
{
    public struct StatusDiff : IAnimeDiff
    {
        public readonly DateTime From { get; }

        public readonly DateTime To { get; }

        public readonly int AnimeId { get; }

        public readonly Status AnimeStatus { get; }

        public StatusDiff( DateTime from, DateTime to, int animeId, Status animeStatus )
        {
            if( from > to ) throw new ArgumentException( "from can't be more recent that to.", nameof( from ) );
            From = from;
            To = to;
            AnimeId = animeId;
            AnimeStatus = animeStatus;
        }

        public IEnumerable<Anime> Apply( IEnumerable<Anime> animes )
        {
            if( animes is null ) throw new ArgumentNullException( nameof( animes ) );

            var @this = new { AnimeId, AnimeStatus };

            Anime anime = animes.FirstOrDefault( anime => anime.Id == @this.AnimeId )
                ?? throw new ArgumentException( $"{nameof( animes )} dosn't have the anime with the Id {@this.AnimeId}" );

            return animes.Select( anime => anime.Id == @this.AnimeId ? anime.Copy( @this.AnimeStatus ) : anime );
        }

        public void Accept( IDiffVisitor visitor ) => visitor.ApplyToDiff( this );
    }
}
