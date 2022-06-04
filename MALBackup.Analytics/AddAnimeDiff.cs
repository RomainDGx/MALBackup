namespace MALBackup.Analytics
{
    public struct AddAnimeDiff : IAnimeDiff
    {
        public readonly DateTime From { get; }

        public readonly DateTime To { get; }

        public readonly Anime Anime { get; }

        public AddAnimeDiff( DateTime from, DateTime to, Anime anime )
        {
            if( from > to ) throw new ArgumentException( "from can't be more recent that to.", nameof( from ) );
            From = from;
            To = to;
            Anime = anime ?? throw new ArgumentNullException( nameof( anime ) );
        }

        public IEnumerable<Anime> Apply( IEnumerable<Anime> animes )
        {
            if( animes is null ) throw new ArgumentNullException( nameof( animes ) );

            var newAnime = Anime;
            if( animes.FirstOrDefault( anime => anime.Id == newAnime.Id ) is not null )
            {
                throw new ArgumentException( $"{nameof( animes )} already has an anime with this Id: {newAnime.Id}", nameof( animes ) );
            }
            return animes.Append( newAnime );
        }

        public void Accept( IDiffVisitor visitor ) => visitor.ApplyToDiff( this );
    }
}
