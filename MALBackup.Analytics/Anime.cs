namespace MALBackup.Analytics
{
    public class Anime
    {
        int _watchedEpisodes;

        public int Id { get; }

        public string Title { get; }

        public int? Episodes { get; }

        public int WatchedEpisodes
        {
            get => _watchedEpisodes;
            set
            {
                if( value < 0 ) throw new ArgumentOutOfRangeException( nameof( WatchedEpisodes ), "Number of watched episodes can't be negative." );
                if( Episodes is not null && Episodes < value ) throw new ArgumentOutOfRangeException( nameof( WatchedEpisodes ), "Watched episodes can't be greater that episodes." );
                _watchedEpisodes = value;
            }
        }

        public Status Status { get; set; }

        public Anime( int id, string title, int? episodes, int watchedEpisodes, Status status )
        {
            ChekArguments( title, episodes, watchedEpisodes );
            Id = id;
            Title = title;
            Episodes = episodes;
            _watchedEpisodes = watchedEpisodes;
            Status = status;
        }

        static void ChekArguments( string title, int? episodes, int watchedEpisodes )
        {
            if( title is null ) throw new ArgumentNullException( nameof( title ) );
            if( watchedEpisodes < 0 ) throw new ArgumentOutOfRangeException( nameof( watchedEpisodes ), "Number of watched episodes can't be negative." );
            if( episodes is not null )
            {
                if( episodes < 0 ) throw new ArgumentOutOfRangeException( nameof( episodes ), "Number of episodes can't be negative." );
                if( episodes < watchedEpisodes ) throw new ArgumentOutOfRangeException( nameof( watchedEpisodes ), "Watched episodes can't be greater that episodes." );
            }
        }

        internal Anime Copy( int watchedEpisodes ) => new( Id, Title, Episodes, watchedEpisodes, Status );

        internal Anime Copy( Status status ) => new( Id, Title, Episodes, _watchedEpisodes, status );
    }
}
