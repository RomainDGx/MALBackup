using CK.Core;
using MALBackup.Core;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MALBackup.App
{
    class Program
    {
        static async Task Main( string[] args )
        {
            var arguments = ValidateArguments( args );

            using HttpClient httpClient = new();
            List<Model.Anime> animes = new();
            List<Model.Anime> newAnimes = new();
            int offset = 0;

            do
            {
                string requestUri = $"https://myanimelist.net/animelist/{arguments.UserName}/load.json?status={arguments.Status}&offset={offset}";

                byte[] data = await SendRequestAsync( httpClient, requestUri );

                animes.AddRange( newAnimes = ParseResponseData( data ) );

                offset += newAnimes.Count;
            }
            while( newAnimes.Count > 0 );

            string filePath = Path.Combine( arguments.TargetFolder, $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss}.json" );
            SaveAnimes( filePath, animes );
        }

        /// <summary>
        /// Check the validity of the program arguments.
        /// </summary>
        /// <param name="args">Array of the program arguments.</param>
        /// <returns>Tuple of the program arguments.</returns>
        static (string UserName, string Status, string TargetFolder) ValidateArguments( string[] args )
        {
            if( args is null ) throw new ArgumentNullException( nameof( args ) );
            if( args.Length != 3 ) throw new ArgumentException( $"Expected 3 arguments but received {args.Length} argument(s)." );
            if( args[1].Length != 1 && args[1][0] is >= '1' and <= '7' ) throw new ArgumentException( $"Invalid status. Expected number between 1 and 7 but received '{args[1]}'." );
            if( !Directory.Exists( args[2] ) ) throw new DirectoryNotFoundException( $"The directory doesn't exist '{args[2]}'." );

            return (args[0], args[1], args[2]);
        }

        static async Task<byte[]> SendRequestAsync( HttpClient client, string requestUri )
        {
            using var response = await client.GetAsync( requestUri );

            ValidateResponse( response );

            return await response.Content.ReadAsByteArrayAsync();
        }

        /// <summary>
        /// Check if the response status is success.
        /// </summary>
        /// <param name="response"></param>
        /// <exception cref="Exception">If the response is not success.</exception>
        static void ValidateResponse( HttpResponseMessage response )
        {
            if( !response.IsSuccessStatusCode )
            {
                var builder = new StringBuilder( "Not success response." )
                    .AppendLine()
                    .Append( "\tStatus code: " )
                    .Append( response.StatusCode )
                    .AppendLine()
                    .Append( "\tRequest uri : " )
                    .Append( response.RequestMessage?.RequestUri );

                throw new Exception( builder.ToString() );
            }
        }

        static List<Model.Anime> ParseResponseData( byte[] data )
        {
            JsonReaderOptions options = new()
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            };
            Utf8JsonReader json = new( data, options );

            // Move inside the first token
            json.Read();

            return AnimeSerializer.DeserializeAnimeList( ref json );
        }

        static void SaveAnimes( string filePath, List<Model.Anime> animes )
        {
            using FileStream stream = File.OpenWrite( filePath );
            using Utf8JsonWriter json = new( stream, new JsonWriterOptions { Indented = true, SkipValidation = true } );

            AnimeSerializer.SerializeAnimeList( json, animes );
        }

        #region Old Code

        static readonly JsonReaderOptions _readerOptions = new()
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        };

        static async Task OldMain( string[] args )
        {
            var now = DateTime.UtcNow.ToString( "yyyy-MM-dd-HH-mm-ss" );
            string fileName = "";

            ActivityMonitor monitor = new();

            try
            {
                monitor.Output.RegisterClient( new ActivityMonitorConsoleClient() );

                using( monitor.Output.RegisterClient( BufferedFileMonitorClient.RegisterClient( monitor, Path.Combine( GetLogFilePath( args ), now + ".log" ), LogFilter.Trace ) ) )
                using( var _ = monitor.OpenTrace( "Run process MALBackupApp.App" ) )
                {
                    CheckArgs( args, out string username, out string status, out string targetFolder );

                    monitor.Info( $"Arguments: '{username}' '{status}' '{targetFolder}'" );

                    var url = $"https://myanimelist.net/animelist/{username}/load.json?status={status}&offset=";

                    fileName = Path.Combine( targetFolder, now + ".json" );
                    using FileStream stream = File.OpenWrite( fileName );

                    monitor.Info( $"Open file {fileName}" );

                    JsonWriterOptions options = new()
                    {
                        Encoder = null,
                        Indented = true,
                        SkipValidation = false
                    };
                    using Utf8JsonWriter writer = new( stream, options );

                    using HttpClient client = new();

                    AnimeList animeList = new();

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
            }
            catch( Exception e )
            {
                monitor.Error( e );
                if( File.Exists( fileName ) ) File.Delete( fileName );
            }
        }

        static bool CheckArgs( string[] args, out string username, out string status, out string targetFolder )
        {
            if( args.Length < 3 ) throw new ArgumentException( $"Expected 3 arguments but received {args.Length}." );
            if( !Regex.IsMatch( args[1], @"^[1-7]$" ) ) throw new ArgumentException( $"Invalid status. Expected number between 1 and 7 and receive {args[1]}" );
            if( !Directory.Exists( args[2] ) ) throw new DirectoryNotFoundException( $"The directory doesn't exist '{args[2]}'" );

            username = args[0];
            status = args[1];
            targetFolder = args[2];

            return true;
        }

        static string GetLogFilePath( string[] args )
            => args.Length >= 4 && Directory.Exists( args[3] )
                ? args[3]
                : Directory.GetCurrentDirectory();
    }

    #endregion
}
