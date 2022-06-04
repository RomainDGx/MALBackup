using FluentAssertions;
using NUnit.Framework;

namespace MALBackup.Analytics.Tests
{
    [TestFixture]
    public class RemoveAnimeDiffTests
    {
        [Test]
        public void To_more_recent_that_from_throws_an_error()
        {
            Func<RemoveAnimeDiff> sut = () => new RemoveAnimeDiff( DateTime.Now, DateTime.Now.AddDays( -1 ), 1 );

            sut.Should().ThrowExactly<ArgumentException>()
                .WithMessage( "from can't be more recent that to. (Parameter 'from')" )
                .WithParameterName( "from" );
        }

        [Test]
        public void New_instance_has_expected_values()
        {
            DateTime from = DateTime.Now.AddDays( -1 );
            DateTime to = DateTime.Now;
            int animeId = new Random().Next( 1000 );

            RemoveAnimeDiff sut = new( from, to, animeId );

            sut.From.Should().Be( from );
            sut.To.Should().Be( to );
            sut.AnimeId.Should().Be( animeId );
        }

        [Test]
        public void Apply_with_null_throws_an_error()
        {
            RemoveAnimeDiff diff = new( DateTime.Now.AddDays( -1 ), DateTime.Now, 1 );

            Func <IEnumerable<Anime>> sut = () => diff.Apply( null! );

            sut.Should().ThrowExactly<ArgumentNullException>().WithParameterName( "animes" );
        }

        [Test]
        public void Apply_diff()
        {
            var animeList = new Anime[]
            {
                new( 1, "Test", 12, 8, Status.Watching ),
                new( 2, "Test2", 24, 24, Status.Completed )
            };

            RemoveAnimeDiff diff = new( DateTime.Now.AddDays( -1 ), DateTime.Now, animeList[0].Id );

            IEnumerable<Anime> sut = diff.Apply( animeList );

            sut.Count().Should().Be( 1 );
            sut.Should().NotBeSameAs( animeList );
            sut.SingleOrDefault( anime => anime.Id == animeList[0].Id ).Should().BeNull();
            sut.SingleOrDefault( anime => anime.Id == animeList[1].Id ).Should().BeSameAs( animeList[1] );
        }

        [Test]
        public void Anime_list_that_not_contains_anime_with_this_Id_throws_an_error()
        {
            var animeList = new Anime[1]
            {
                new( 1, "Test", 12, 5, Status.Watching )
            };
            int animeId = 2;

            RemoveAnimeDiff diff = new( DateTime.Now.AddDays( -1 ), DateTime.Now, animeId );

            Func<IEnumerable<Anime>> sut = () => diff.Apply( animeList );

            sut.Should().ThrowExactly<ArgumentException>( $"animes doesn't contain any anime with this Id: {animeId} (Parameter 'from')" );
        }
    }
}
