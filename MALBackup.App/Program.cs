using CK.Core;
using MALBackup.Core;
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
        static readonly JsonReaderOptions _readerOptions = new()
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        };

        static async Task Main( string[] args )
        {
            var now = DateTime.UtcNow.ToString( "yyyy-MM-dd-HH-mm-ss" );

            var monitor = new ActivityMonitor();

            monitor.Output.RegisterClient( new ActivityMonitorConsoleClient() );
            using( monitor.Output.RegisterClient( BufferedFileMonitorClient.RegisterClient( monitor, Path.Combine( "D:\\dev\\Perso\\MyAnimeList\\MALBackup\\MALBackup.App\\Logs\\", now + ".log" ), LogFilter.Trace ) ) )
            using( var _ = monitor.OpenTrace( "Run process MALBackupApp.App" ) )
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

                    monitor.Info( $"Arguments: '{username}' '{status}' '{targetFolder}'" );

                    var url = $"https://myanimelist.net/animelist/{username}/load.json?status={status}&offset=";

                    fileName = Path.Combine( targetFolder, now + ".json" );
                    using FileStream stream = File.OpenWrite( fileName );

                    monitor.Info( $"Open file {fileName}" );

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

                        monitor.Info( $"Sending request at {url + animeList.Count.ToString()}" );

                        if( !data.IsSuccessStatusCode )
                        {
                            throw new HttpRequestException( $"Http request error: code {data.StatusCode}" );
                        }
                        monitor.Info( $"Http request successfully sended" );

                        byte[] byteData = await data.Content.ReadAsByteArrayAsync();

                        animeList.Concat( new Utf8JsonReader( byteData, _readerOptions ) );

                        monitor.Info( "Successfully anime data parsing" );

                        // One load of data from myanimelist contains 300 entities
                    } while( animeList.Count % 300 == 0 );

                    animeList.Save( writer );

                    monitor.Info( "Animelist was successfully saved" );
                }
                catch( Exception e )
                {
                    monitor.Error( e );
                    if( File.Exists( fileName ) ) File.Delete( fileName );
                }
            }
        }
    }
}
