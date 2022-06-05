using System.Text;
using System.Text.Json;

namespace MALBackup.App
{
    class Program
    {
        static async Task Main( string[] args )
        {
            var (userName, status, targetFolder) = ValidateArguments( args );

            using HttpClient httpClient = new();
            List<Model.Anime> animes = new();
            List<Model.Anime> newAnimes = new();
            int offset = 0;

            do
            {
                string requestUri = $"https://myanimelist.net/animelist/{userName}/load.json?status={status}&offset={offset}";

                byte[] data = await SendRequestAsync( httpClient, requestUri );

                animes.AddRange( newAnimes = ParseResponseData( data ) );

                offset += newAnimes.Count;
            }
            while( newAnimes.Count > 0 );

            string filePath = Path.Combine( targetFolder, $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss}.json" );
            SaveAnimes( filePath, animes );
        }

        /// <summary>
        /// Check the validity of the program arguments.
        /// </summary>
        /// <param name="args">Array of the program arguments.</param>
        /// <returns>Tuple of the program arguments.</returns>
        static (string UserName, string Status, string TargetFolder) ValidateArguments( string[] args )
        {
            if( args is null )
            {
                throw new ArgumentNullException( nameof( args ) );
            }
            if( args.Length != 3 )
            {
                throw new ArgumentException( $"Expected 3 arguments but received {args.Length} argument(s)." );
            }
            if( args[1].Length != 1 || args[1][0] is < '1' or > '7' )
            {
                throw new ArgumentException( $"Invalid status. Expected number between 1 and 7 but received '{args[1]}'." );
            }
            if( !Directory.Exists( args[2] ) )
            {
                throw new DirectoryNotFoundException( $"The directory doesn't exist '{args[2]}'." );
            }

            return (args[0], args[1], args[2]);
        }

        static async Task<byte[]> SendRequestAsync( HttpClient httpClient, string requestUri )
        {
            using var response = await httpClient.GetAsync( requestUri );

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

        /// <summary>
        /// Save the animes in Json format in the specified file.
        /// </summary>
        static void SaveAnimes( string filePath, List<Model.Anime> animes )
        {
            using FileStream stream = File.OpenWrite( filePath );
            using Utf8JsonWriter json = new( stream, new JsonWriterOptions { Indented = true, SkipValidation = true } );

            AnimeSerializer.SerializeAnimeList( json, animes );
        }
    }
}
