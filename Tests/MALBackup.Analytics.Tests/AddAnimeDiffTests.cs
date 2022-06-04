using FluentAssertions;
using NUnit.Framework;

namespace MALBackup.Analytics.Tests
{
    [TestFixture]
    public class AddAnimeDiffTests
    {
        [Test]
        public void Null_reference_anime_throws_an_error()
        {
            Func<AddAnimeDiff> sut = () => new AddAnimeDiff( DateTime.Now.AddDays( -1 ), DateTime.Now, null! );

            sut.Should().ThrowExactly<ArgumentNullException>().WithParameterName( "anime" );
        }

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
            Anime anime = new( 1, "Test", 12, 8, Status.Watching );

            AddAnimeDiff sut = new( from, to, anime );

            sut.From.Should().Be( from );
            sut.To.Should().Be( to );
            sut.Anime.Should().BeSameAs( anime );
        }

        [Test]
        public void Apply_with_null_throws_an_error()
        {
            AddAnimeDiff diff = new( DateTime.Now.AddDays( -1 ), DateTime.Now, new( 1, "Test", 21, 5, Status.Watching ) );

            Func<IEnumerable<Anime>> sut = () => diff.Apply( null! );

            sut.Should().ThrowExactly<ArgumentNullException>().WithParameterName( "animes" );
        }

        [Test]
        public void Apply_diff()
        {
            var animeList = new Anime[1]
            {
                new( 1, "Test", 12, 8, Status.Watching )
            };
            Anime newAnime = new( 2, "Test2", 24, 24, Status.Completed );

            AddAnimeDiff diff = new( DateTime.Now.AddDays( -1 ), DateTime.Now, newAnime );

            IEnumerable<Anime> sut = diff.Apply( animeList );

            sut.Count().Should().Be( 2 );
            sut.Should().NotBeSameAs( animeList );
            sut.SingleOrDefault( anime => anime.Id == animeList[0].Id ).Should().BeSameAs( animeList[0] );
            sut.SingleOrDefault( anime => anime.Id == newAnime.Id ).Should().BeSameAs( newAnime );
        }

        [Test]
        public void Anime_list_that_already_has_anime_with_this_Id()
        {
            var animeList = new Anime[1]
            {
                new( 1, "Test", 12, 5, Status.Watching )
            };
            Anime newAnimes = new( 1, "Test", 12, 5, Status.Watching );

            AddAnimeDiff diff = new( DateTime.Now.AddDays( -1 ), DateTime.Now, newAnimes );

            Func<IEnumerable<Anime>> sut = () => diff.Apply( animeList );

            sut.Should().ThrowExactly<ArgumentException>( $"animes already has this anime: {newAnimes.Id} (Parameter 'animes')" );
        }
    }
}
