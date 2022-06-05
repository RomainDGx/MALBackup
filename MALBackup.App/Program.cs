using MALBackup.Model;
using System.Text;
using System.Text.Json;

namespace MALBackup.App
{
    class Program
    {
        static async Task Main( string[] args )
        {
            var (clientId, userName, targetFolder) = ValidateArguments( args );

            using HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Add( "X-MAL-CLIENT-ID", clientId );

            string? requestUri = $"https://api.myanimelist.net/v2/users/{userName}/animelist?sort=anime_title&limit=1000&offset=0&nsfw=true&fields=id,title,num_episodes,list_status{{status,num_episodes_watched,num_times_rewatched,rewatch_value}}";

            List<Anime> animes = new();

            while( requestUri is not null )
            {
                using HttpResponseMessage response = await SendRequestAsync( httpClient, requestUri );

                ResponsePayload? payload = await ParseResponseContentAsync( await response.Content.ReadAsStreamAsync() );
                ValidateResponsePayload( payload );

                animes.AddRange( payload!.Data!.Select( AnimeListData.GetAnime ) );

                requestUri = payload!.Paging!.Next;
            }

            string filePath = Path.Combine( targetFolder, $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss}.json" );
            await SaveAnimesAsync( filePath, animes );
        }

        /// <summary>
        /// Check the validity of the program arguments.
        /// </summary>
        /// <param name="args">Array of the program arguments.</param>
        /// <returns>Tuple of the program arguments.</returns>
        static (string ClientId, string UserName, string TargetFolder) ValidateArguments( string[] args )
        {
            if( args is null )
            {
                throw new ArgumentNullException( nameof( args ) );
            }
            if( args.Length != 3 )
            {
                throw new ArgumentException( $"Expected 3 arguments but received {args.Length} argument(s)." );
            }
            if( !Directory.Exists( args[2] ) )
            {
                throw new DirectoryNotFoundException( $"The directory doesn't exist '{args[2]}'." );
            }
            return (args[0], args[1], args[2]);
        }

        static async Task<HttpResponseMessage> SendRequestAsync( HttpClient httpClient, string requestUri )
        {
            HttpResponseMessage response = await httpClient.GetAsync( requestUri );

            await ValidateHttpResponseAsync( response );

            return response;
        }

        /// <summary>
        /// Check if the response status is success.
        /// </summary>
        /// <param name="response"></param>
        /// <exception cref="Exception">If the response is not success.</exception>
        static async Task ValidateHttpResponseAsync( HttpResponseMessage response )
        {
            if( !response.IsSuccessStatusCode )
            {
                var builder = new StringBuilder( "Not success response." )
                    .AppendLine()
                    .Append( "\t- Status code: " )
                    .Append( response.StatusCode )
                    .AppendLine()
                    .Append( "\t- Request uri : " )
                    .Append( response.RequestMessage?.RequestUri )
                    .AppendLine()
                    .Append( "\t- Content : " )
                    .Append( await response.Content.ReadAsStringAsync() );

                throw new HttpRequestException( builder.ToString() );
            }
        }

        static async Task<ResponsePayload?> ParseResponseContentAsync( Stream data )
        {
            JsonSerializerOptions options = new()
            {
                Converters = { StatusConverter.Instance }
            };
            return await JsonSerializer.DeserializeAsync<ResponsePayload>( data, options );
        }

        static void ValidateResponsePayload( ResponsePayload? response )
        {
            if( response is null )
            {
                throw new ArgumentNullException( nameof( response ) );
            }
            if( response.Data is null )
            {
                throw new InvalidDataException( $"{nameof( response )}.{nameof( response.Data )} is null." );
            }
            if( response.Paging is null )
            {
                throw new InvalidDataException( $"{nameof( response )}.{nameof( response.Paging )} is null." );
            }
        }

        /// <summary>
        /// Save the animes in Json format in the specified file.
        /// </summary>
        static async Task SaveAnimesAsync( string filePath, List<Anime> animes )
        {
            using FileStream stream = File.OpenWrite( filePath );

            await JsonSerializer.SerializeAsync( stream, animes, new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { StatusConverter.Instance }
            } );
        }
    }
}
