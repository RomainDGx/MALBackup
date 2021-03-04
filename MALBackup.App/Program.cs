using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MALBackup.App
{
    class Program
    {
        static readonly JsonReaderOptions _readerOptions = new JsonReaderOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        };

        static async Task Main( string[] args )
        {
            string fileName = "";
            try
            {
                if( args.Length < 3 ) throw new ArgumentException( $"Expected 3 arguments but received {args.Length}." );
                if( !Regex.IsMatch( args[1], @"^[1-7]$" ) ) throw new ArgumentException( $"Invalid status. Expected number between 1 and 7 and receive {args[1]}" );
                if( !Directory.Exists( args[2] ) ) throw new DirectoryNotFoundException( $"Directory not exists '{args[2]}'" );

                string username = args[0];
                string status = args[1];
                string targetFolder = args[2];

                // Tip from this website : https://malscraper.azurewebsites.net/
                var url = $"https://myanimelist.net/animelist/{username}/load.json?status={status}&offset=";

                fileName = Path.Combine( targetFolder, DateTime.UtcNow.ToString( "yyyyy-MM-dd-HH-mm-ss" ) + ".json" );
                using FileStream stream = File.OpenWrite( fileName );

                var options = new JsonWriterOptions()
                {
                    Encoder = null,
                    Indented = true,
                    SkipValidation = false
                };
                using var writter = new Utf8JsonWriter( stream, options );

                using var client = new HttpClient();

                writter.WriteStartArray();

                // Count total anime
                int count = 0;
                do
                {
                    HttpResponseMessage data = await client.GetAsync( url + count.ToString() );
                    if( !data.IsSuccessStatusCode ) throw new HttpRequestException( $"Http error : code {data.StatusCode}" );

                    byte[] byteData = await data.Content.ReadAsByteArrayAsync();

                    count += DataAdder( writter, byteData );

                    // One load of data from myanimelist contains 300 entities
                } while( count % 300 == 0 );

                writter.WriteEndArray();
            }
            catch( Exception e )
            {
                Console.WriteLine( $"Error : {e.Message}" );
                if( File.Exists( fileName ) ) File.Delete( fileName );
            }
        }

        /// <summary>
        /// Add <paramref name="data"/> in <paramref name="writter"/> content.
        /// </summary>
        /// <returns>Number of new elements.</returns>
        static int DataAdder( Utf8JsonWriter writter, byte[] data )
        {
            int count = 0;

            Utf8JsonReader reader = new Utf8JsonReader( data, _readerOptions );

            // TODO : Test if throw exception if reader contains void array
            reader.Read(); // Open start array
            reader.Read(); // Open first oject
            writter.WriteStartObject();

            while( reader.Read() && reader.TokenType != JsonTokenType.EndArray )
            {
                if( reader.TokenType == JsonTokenType.EndObject )
                {
                    writter.WriteEndObject();
                    count++;
                    reader.Read(); // Open next object or end array

                    if( reader.TokenType == JsonTokenType.EndArray )
                    {
                        break;
                    }

                    writter.WriteStartObject();
                    reader.Read(); // Open property
                }

                string propertyName = reader.GetString();

                reader.Read(); // go to value

                switch( propertyName )
                {
                    case "status":
                    case "score":
                    case "is_rewatching":
                    case "num_watched_episodes":
                    case "anime_num_episodes":
                    case "anime_airing_status":
                    case "anime_id":
                        writter.WriteNumber( propertyName, reader.GetInt32() );
                        break;

                    case "has_episode_video":
                    case "has_promotion_video":
                    case "has_video":
                    case "is_added_to_list":
                        writter.WriteBoolean( propertyName, reader.GetBoolean() );
                        break;

                    case "tags":
                    case "anime_title":
                    case "video_url":
                    case "anime_url":
                    case "anime_image_path":
                    case "anime_media_type_string":
                    case "anime_mpaa_rating_string":
                    case "anime_start_date_string":
                    case "anime_end_date_string":
                    case "storage_string":
                    case "priority_string":
                        writter.WriteString( propertyName, reader.GetString() );
                        break;

                    case "anime_studios":
                    case "anime_licensors":
                    case "anime_season":
                    case "start_date_string":
                    case "finish_date_string":
                    case "days_string":
                        // TODO : Find the value of the types by checking the checkboxes : https://myanimelist.net/editprofile.php?go=listpreferences
                        writter.WriteNull( propertyName );
                        break;
                }
            }
            return count;
        }
    }
}
