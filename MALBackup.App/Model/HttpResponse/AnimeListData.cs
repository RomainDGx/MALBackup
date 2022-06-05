using MALBackup.Model;
using System.Text.Json.Serialization;

namespace MALBackup.App
{
    internal class AnimeListData
    {
        [JsonPropertyName( "node" )]
        public Anime? Anime { get; set; }

        [JsonPropertyName( "list_status" )]
        public UserStatus? UserListStatus { get; set; }

        public static Anime GetAnime( AnimeListData data )
        {
            ValidateAnimeListData( data );
            data.Anime!.UserStatus = data.UserListStatus;
            return data.Anime!;
        }

        public static void ValidateAnimeListData( AnimeListData? data )
        {
            if( data is null )
            {
                throw new ArgumentNullException( nameof( data ) );
            }
            if( data.Anime is null )
            {
                throw new InvalidDataException( $"{nameof( data )}.{nameof( data.Anime )} is null." );
            }
            if( data.UserListStatus is null )
            {
                throw new InvalidDataException( $"{nameof( data )}.{nameof( data.UserListStatus )} is null." );
            }
        }
    }
}
