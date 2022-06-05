using MALBackup.Model;
using System.Diagnostics;
using System.Text.Json;

namespace MALBackup.App
{
    internal static class AnimeSerializer
    {
        public static Anime DeserializeAnime( ref Utf8JsonReader json )
        {
            if( json.TokenType != JsonTokenType.StartObject )
            {
                throw new InvalidDataException( "Anime entity should start with open curly bracket." );
            }

            Anime anime = new();

            while( json.Read() && json.TokenType != JsonTokenType.EndObject )
            {
                Debug.Assert( json.TokenType == JsonTokenType.PropertyName );

                string? propertyName = json.GetString();
                Debug.Assert( propertyName is not null );

                // Move to the property value
                json.Read();

                switch( propertyName )
                {
                    case "anime_id":
                        {
                            anime.Id = json.GetInt32();
                            break;
                        }
                    case "anime_title":
                        {
                            anime.Title = json.GetString();
                            if( anime.Title is null )
                            {
                                throw new InvalidDataException( "Anime title shouldn't be null." );
                            }
                            break;
                        }
                    case "anime_num_episodes":
                        {
                            anime.Episodes = json.GetInt32();
                            break;
                        }
                    case "num_watched_episodes":
                        {
                            anime.WatchedEpisodes = json.GetInt32();
                            break;
                        }
                    case "status":
                        {
                            var status = (Status)json.GetInt32();
                            if( !Enum.IsDefined( status ) )
                            {
                                throw new InvalidDataException( $"Invalid status: {status}" );
                            }
                            anime.Status = status;
                            break;
                        }
                    default:
                        {
                            // Skip the value of the useless property
                            json.Skip();
                            break;
                        }
                }
            }
            return anime;
        }

        public static List<Anime> DeserializeAnimeList( ref Utf8JsonReader json )
        {
            if( json.TokenType != JsonTokenType.StartArray )
            {
                throw new InvalidDataException( "Anime list should start with open bracket." );
            }

            List<Anime> animes = new();

            while( json.Read() && json.TokenType != JsonTokenType.EndArray )
            {
                animes.Add( DeserializeAnime( ref json ) );
            }
            return animes;
        }

        public static void SerializeAnime( Utf8JsonWriter json, Anime anime )
        {
            if( json is null ) throw new ArgumentNullException( nameof( json ) );
            if( anime is null ) throw new ArgumentNullException( nameof( anime ) );

            json.WriteStartObject();
            json.WriteNumber( "anime_id", anime.Id );
            json.WriteString( "anime_title", anime.Title );
            json.WriteNumber( "anime_num_episodes", anime.Episodes );
            json.WriteNumber( "num_watched_episodes", anime.WatchedEpisodes );
            json.WriteNumber( "status", (int)anime.Status );
            json.WriteEndObject();
        }

        public static void SerializeAnimeList( Utf8JsonWriter json, List<Anime> animes )
        {
            if( json is null ) throw new ArgumentNullException( nameof( json ) );
            if( animes is null ) throw new ArgumentNullException( nameof( animes ) );

            json.WriteStartArray();

            foreach( Anime anime in animes )
            {
                SerializeAnime( json, anime );
            }
            json.WriteEndArray();
        }
    }
}
