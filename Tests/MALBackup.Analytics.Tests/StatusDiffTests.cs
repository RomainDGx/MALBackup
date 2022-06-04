using FluentAssertions;
using NUnit.Framework;

namespace MALBackup.Analytics.Tests
{
    [TestFixture]
    public class StatusDiffTests
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
            Array statusValues = Enum.GetValues( typeof( Status ) );
            Status status = (Status)statusValues.GetValue( new Random().Next( statusValues.Length ) )!;

            StatusDiff sut = new( from, to, animeId, status );

            sut.From.Should().Be( from );
            sut.To.Should().Be( to );
            sut.AnimeId.Should().Be( animeId );
            sut.AnimeStatus.Should().Be( status );
        }

        [Test]
        public void Apply_with_null_throws_an_error()
        {
            StatusDiff diff = new( DateTime.Now.AddDays( -1 ), DateTime.Now, 1, Status.Watching );

            Func<IEnumerable<Anime>> sut = () => diff.Apply( null! );

            sut.Should().ThrowExactly<ArgumentNullException>().WithParameterName( "animes" );
        }

        [Test]
        public void Apply_with_anime_not_in_anime_list()
        {
            int animeId = new Random().Next( 1, 10_000 );
            StatusDiff diff = new( DateTime.Now.AddDays( -1 ), DateTime.Now, animeId, Status.Watching );

            Func<IEnumerable<Anime>> sut = () => diff.Apply( Array.Empty<Anime>() );

            sut.Should().ThrowExactly<ArgumentException>()
                .WithMessage( $"animes dosn't have the anime with the Id {animeId}" );
        }

        [Test]
        public void Apply_diff()
        {
            var animeList = new Anime[]
            {
                new( 1, "Test", 24, 12, Status.Watching ),
                new( 2, "Test2", 12, 12, Status.Completed )
            };
            var newStatus = Status.OnHold;
            StatusDiff diff = new( DateTime.Now.AddDays( -2 ), DateTime.Now, animeList[0].Id, newStatus );

            IEnumerable<Anime> sut = diff.Apply( animeList );

            sut.Should().NotBeSameAs( animeList );
            sut.Count().Should().Be( animeList.Length );
            sut.First().Should().NotBeSameAs( animeList[0] );
            sut.First().Id.Should().Be( animeList[0].Id );
            sut.First().Title.Should().Be( animeList[0].Title );
            sut.First().Episodes.Should().Be( animeList[0].Episodes );
            sut.First().WatchedEpisodes.Should().Be( animeList[0].WatchedEpisodes );
            sut.First().Status.Should().Be( newStatus );
            sut.Last().Should().BeSameAs( animeList[1] );
        }
    }
}
