namespace MALBackup.Analytics
{
    public interface IDiffVisitor
    {
        public void ApplyToDiff( AddAnimeDiff diff );

        public void ApplyToDiff( RemoveAnimeDiff diff );

        public void ApplyToDiff( StatusDiff diff );

        public void ApplyToDiff( WatchedEpisodesDiff diff );
    }
}
