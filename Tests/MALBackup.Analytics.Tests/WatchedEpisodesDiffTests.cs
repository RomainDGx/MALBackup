using FluentAssertions;
using NUnit.Framework;

namespace MALBackup.Analytics.Tests
{
    [TestFixture]
    public class WatchedEpisodesDiffTests
    {
        [Test]
        public void To_more_recent_that_from_throws_an_error()
        {
            Func<AddAnimeDiff> sut = () => new AddAnimeDiff( DateTime.Now, DateTime.Now.AddDays( -1 ), null! );

            sut.Should().ThrowExactly<ArgumentException>()
                .WithMessage( "from can't be more recent that to. (Parameter 'from')" )
                .WithParameterName( "from" );
        }

        [Test]
        public void New_instance_has_expected_values()
        {
            DateTime from = DateTime.Now.AddDays( -1 );
            DateTime to = DateTime.Now;
            int animeId = new Random().Next( 1, 10_000 );
            int episodesDiff = new Random().Next( 1, 10_000 );

            WatchedEpisodesDiff sut = new( from, to, animeId, episodesDiff );

            sut.From.Should().Be( from );
            sut.To.Should().Be( to );
            sut.AnimeId.Should().Be( animeId );
            sut.EpisodesDiff.Should().Be( episodesDiff );
        }

        [Test]
        public void Apply_with_null_throws_an_error()
        {
            WatchedEpisodesDiff diff = new( DateTime.Now.AddDays( -1 ), DateTime.Now, 1, 5 );

            Func<IEnumerable<Anime>> sut = () => diff.Apply( null! );

            sut.Should().ThrowExactly<ArgumentNullException>().WithParameterName( "animes" );
        }

        [Test]
        public void Apply_with_anime_not_in_anime_list()
        {
            int animeId = new Random().Next( 1, 10_000 );
            WatchedEpisodesDiff diff = new( DateTime.Now.AddDays( -1 ), DateTime.Now, animeId, 5 );

            Func<IEnumerable<Anime>> sut = () => diff.Apply( Array.Empty<Anime>() );

            sut.Should().ThrowExactly<ArgumentException>()
                .WithMessage( $"animes dosn't have the anime with the Id {animeId}" );
        }

        [TestCase( 1, 5, 6 )]
        [TestCase( 10, 1, 11 )]
        [TestCase( 1, -1, 0 )]
        [TestCase( 24, -6, 18 )]
        public void Apply_diff( int watchedEpisodesBase, int watchedEpisodesDiff, int watchedEpisodesResult )
        {
            var animeList = new Anime[]
            {
                new( 1, "Test", int.MaxValue, watchedEpisodesBase, Status.Watching ),
                new( 2, "Test2", 24, 24, Status.Completed )
            };

            WatchedEpisodesDiff diff = new( DateTime.Now.AddDays( -2 ), DateTime.Now, animeList[0].Id, watchedEpisodesDiff );

            IEnumerable<Anime> sut = diff.Apply( animeList );

            sut.Should().NotBeSameAs( animeList );
            sut.Count().Should().Be( animeList.Length );
            sut.First().Should().NotBeSameAs( animeList[0] );
            sut.First().Id.Should().Be( animeList[0].Id );
            sut.First().Title.Should().Be( animeList[0].Title );
            sut.First().Episodes.Should().Be( animeList[0].Episodes );
            sut.First().WatchedEpisodes.Should().Be( watchedEpisodesResult );
            sut.First().Status.Should().Be( animeList[0].Status );
            sut.Last().Should().BeSameAs( animeList[1] );
        }
    }
}
