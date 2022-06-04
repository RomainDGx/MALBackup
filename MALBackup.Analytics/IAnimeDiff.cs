namespace MALBackup.Analytics
{
    public interface IAnimeDiff
    {
        public DateTime From { get; }

        public DateTime To { get; }

        public IEnumerable<Anime> Apply( IEnumerable<Anime> animes );

        public void Accept( IDiffVisitor visitor );
    }
}
