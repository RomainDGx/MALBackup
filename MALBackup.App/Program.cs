using MALBackup.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MALBackup.App
{
    class Program
    {
        static readonly JsonReaderOptions _readerOptions = new()
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

                fileName = Path.Combine( targetFolder, DateTime.UtcNow.ToString( "yyyy-MM-dd-HH-mm-ss" ) + ".json" );
                using FileStream stream = File.OpenWrite( fileName );

                var options = new JsonWriterOptions()
                {
                    Encoder = null,
                    Indented = true,
                    SkipValidation = false
                };
                using var writer = new Utf8JsonWriter( stream, options );

                using var client = new HttpClient();

                var animeList = new AnimeList();

                do
                {
                    HttpResponseMessage data = await client.GetAsync( url + animeList.Count.ToString() );
                    if( !data.IsSuccessStatusCode )
                    {
                        throw new HttpRequestException( $"Http error : code {data.StatusCode}" );
                    }

                    byte[] byteData = await data.Content.ReadAsByteArrayAsync();
                    
                    animeList.Concat( new Utf8JsonReader( byteData, _readerOptions ) );

                // One load of data from myanimelist contains 300 entities
                } while( animeList.Count % 300 == 0 );

                animeList.Save( writer );
            }
            catch( Exception e )
            {
                Console.WriteLine( $"Error : {e.Message}" );
                Debug.Write( e.StackTrace );
                if( File.Exists( fileName ) ) File.Delete( fileName );
            }
        }
    }
}
