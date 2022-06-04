namespace MALBackup.Analytics
{
    public struct WatchedEpisodesDiff : IAnimeDiff
    {
        public readonly DateTime From { get; }

        public readonly DateTime To { get; }

        public readonly int AnimeId { get; }

        public readonly int EpisodesDiff { get; }

        public WatchedEpisodesDiff( DateTime from, DateTime to, int animeId, int episodesDiff )
        {
            if( from > to ) throw new ArgumentException( "from can't be more recent that to.", nameof( from ) );
            From = from;
            To = to;
            AnimeId = animeId;
            EpisodesDiff = episodesDiff;
        }

        public IEnumerable<Anime> Apply( IEnumerable<Anime> animes )
        {
            if( animes is null ) throw new ArgumentNullException( nameof( animes ) );

            var @this = new { AnimeId, EpisodesDiff };

            Anime anime = animes.FirstOrDefault( anime => anime.Id == @this.AnimeId )
                ?? throw new ArgumentException( $"{nameof( animes )} dosn't have the anime with the Id {@this.AnimeId}" );

            return animes.Select( anime => anime.Id == @this.AnimeId ? anime.Copy( anime.WatchedEpisodes + @this.EpisodesDiff ) : anime );
        }

        public void Accept( IDiffVisitor visitor ) => visitor.ApplyToDiff( this );
    }
}
