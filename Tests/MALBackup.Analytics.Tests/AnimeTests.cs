using FluentAssertions;
using NUnit.Framework;

namespace MALBackup.Analytics.Tests
{
    [TestFixture]
    public class AnimeTests
    {
        [Test]
        public void Null_title_throws_an_error()
        {
            Func<Anime> sut = () => new Anime( 1, null!, 12, 5, Status.Watching );

            sut.Should().ThrowExactly<ArgumentNullException>().WithParameterName( "title" );
        }

        [Test]
        public void Negative_episodes_throws_an_error()
        {
            Func<Anime> sut = () => new Anime( 1, "Test", -5, 2, Status.Watching );

            sut.Should().ThrowExactly<ArgumentOutOfRangeException>()
                .WithMessage( "Number of episodes can't be negative. (Parameter 'episodes')" )
                .WithParameterName( "episodes" );
        }

        [Test]
        public void Negative_watchedEpisodes_throws_an_error()
        {
            Func<Anime> func = () => new Anime( 1, "Test", 5, -2, Status.Watching );

            func.Should().ThrowExactly<ArgumentOutOfRangeException>()
                .WithMessage( "Number of watched episodes can't be negative. (Parameter 'watchedEpisodes')" )
                .WithParameterName( "watchedEpisodes" );
        }

        [Test]
        public void Negative_watchedEpisodes_set_throws_an_error()
        {
            Action sut = () => new Anime( 1, "Test", 12, 5, Status.Watching ).WatchedEpisodes = -5;

            sut.Should().ThrowExactly<ArgumentOutOfRangeException>()
                .WithMessage( "Number of watched episodes can't be negative. (Parameter 'watchedEpisodes')" )
                .WithParameterName( "WatchedEpisodes" );
        }

        [Test]
        public void WatchedEpisodes_set_greater_that_episodes_throws_an_error()
        {
            Action sut = () => new Anime( 1, "Test", 12, 5, Status.Watching ).WatchedEpisodes = 24;

            sut.Should().ThrowExactly<ArgumentOutOfRangeException>()
                .WithMessage( "Watched episodes can't be greater that episodes. (Parameter 'watchedEpisodes')" )
                .WithParameterName( "WatchedEpisodes" );
        }

        [Test]
        public void WatchedEpisodes_greater_that_episodes_throws_an_error()
        {
            Func<Anime> sut = () => new Anime( 1, "Test", 12, 24, Status.Completed );

            sut.Should().ThrowExactly<ArgumentOutOfRangeException>()
                .WithMessage( "Watched episodes can't be greater that episodes. (Parameter 'watchedEpisodes')" )
                .WithParameterName( "watchedEpisodes" );
        }

        [Test]
        public void New_instance_has_expected_values()
        {
            int id = 5;
            string title = "Test";
            int episodes = 12;
            int watchedEpisodes = 5;
            Status status = Status.Watching;

            Anime sut = new( id, title, episodes, watchedEpisodes, status );

            sut.Should().NotBeNull();
            sut.Id.Should().Be( id );
            sut.Title.Should().Be( title );
            sut.Episodes.Should().Be( episodes );
            sut.WatchedEpisodes.Should().Be( watchedEpisodes );
            sut.Status.Should().Be( status );

            sut = new( id, title, null, watchedEpisodes, status );

            sut.Should().NotBeNull();
            sut.Id.Should().Be( id );
            sut.Title.Should().Be( title );
            sut.Episodes.Should().BeNull();
            sut.WatchedEpisodes.Should().Be( watchedEpisodes );
            sut.Status.Should().Be( status );

            int newWatchedEpisodes = 12;
            sut.WatchedEpisodes = newWatchedEpisodes;
            sut.WatchedEpisodes.Should().Be( newWatchedEpisodes );
        }
    }
}
