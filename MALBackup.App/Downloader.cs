using CK.Core;
using MALBackup.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MALBackup.App
{
    public class Downloader : IDisposable
    {
        readonly IActivityMonitor _monitor;
        readonly DownloaderConfiguration _options;
        readonly JsonSerializerOptions _serializerOptions;
        readonly HttpClient _httpClient;

        Downloader( IActivityMonitor monitor, DownloaderConfiguration options )
        {
            _monitor = monitor;
            _options = options;
            _serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new StatusConverter() }
            };
            _httpClient = new HttpClient
            {
                DefaultRequestHeaders = { { "X-MAL-CLIENT-ID", options.MALClientId } },
            };
        }

#pragma warning disable CS8601 // Possible null reference assignment.
        public static Downloader? TryCreate( IActivityMonitor monitor, IConfiguration config )
        {
            var conf = new DownloaderConfiguration();

            if( string.IsNullOrEmpty( conf.MALClientId = config["MALClientId"] ) )
            {
                monitor.Error( "Configuration parameter 'MALClientId' is missing." );
                return null;
            }

            if( string.IsNullOrEmpty( conf.MALUserName = config["MALUserName"] ) )
            {
                monitor.Error( "Configuration parameter 'MALUserName' is missing." );
                return null;
            }

            if( !int.TryParse( config["Retry"], out int retry ) )
            {
                monitor.Error( "Configuration parameter 'Retry' is missing." );
                return null;
            }
            conf.Retry = retry;

            if( string.IsNullOrEmpty( conf.OutputDirectory = config["OutputDirectory"] ) )
            {
                monitor.Error( "Configuration parameter 'OutputDirectory' is missing." );
                return null;
            }

            return new Downloader( monitor, conf );
        }
#pragma warning restore CS8601 // Possible null reference assignment.

        public async Task ExecuteAync( CancellationToken token = default )
        {
            _monitor.Info( $"Downloading anime list." );

            var animes = new List<Anime>();
            string? requestUri = $"https://api.myanimelist.net/v2/users/{_options.MALUserName}/animelist?sort=anime_title&limit=1000&offset=0&nsfw=true&fields=id,title,num_episodes,list_status{{status,num_episodes_watched,num_times_rewatched,rewatch_value}}";
            int tries = 1;

            while( requestUri is not null && !token.IsCancellationRequested && tries <= _options.Retry )
            {
                try
                {
                    using var response = await _httpClient.GetAsync( requestUri, token );
                    await EnsureResponseAsync( response, token );

                    using var stream = await response.Content.ReadAsStreamAsync( token );
                    var content = await JsonSerializer.DeserializeAsync<ResponsePayload>( stream, _serializerOptions, token );
                    content = Check( content );

                    animes.AddRange( Check( content.Data ).Select( node =>
                    {
                        node.Anime = Check( node.Anime );
                        node.Anime.UserStatus = Check( node.UserListStatus );
                        return node.Anime;
                    } ) );

                    requestUri = Check( content.Paging ).Next;
                }
                catch( Exception ex )
                {
                    if( ex is MALApiResponseException malEx )
                    {
                        var builder = new StringBuilder()
                            .AppendLine( malEx.Message )
                            .Append( "Status code: " )
                            .AppendLine( malEx.StatusCode.ToString() )
                            .Append( "Response content: " )
                            .AppendLine( malEx.Response?.ToString() );

                        _monitor.Error( builder.ToString(), ex );
                    }
                    else
                    {
                        _monitor.Error( ex );
                    }
                    await Task.Delay( TimeSpan.FromSeconds( tries++ * 5 ), token );
                }
            }

            if( requestUri is null )
            {
                await SaveAnimesAsync( animes, token );
                _monitor.Info( "The operation was successfully competed." );
            }
            else
            {
                _monitor.Warn( "The operation was unsuccessful." );
            }
        }

        async Task EnsureResponseAsync( HttpResponseMessage response, CancellationToken token )
        {
            if( !response.IsSuccessStatusCode )
            {
                using var stream = await response.Content.ReadAsStreamAsync( token );
                var errorResponse = await JsonSerializer.DeserializeAsync<ErrorResponse>( stream, _serializerOptions, token );

                throw new MALApiResponseException(
                    response.StatusCode,
                    errorResponse,
                    "An error has occurend when requesting." );
            }
        }

        /// <summary>
        /// Check if the given value is null. If it's null, then an error is throw.
        /// </summary>
        /// <param name="value">The value to check.</param>
        static T Check<T>( [NotNullIfNotNull( nameof( value ) )] T? value )
        {
            return value ?? throw new InvalidOperationException( "The value is null." );
        }

        /// <summary>
        /// Save the animes in Json format in the specified file.
        /// </summary>
        async Task SaveAnimesAsync( List<Anime> animes, CancellationToken token )
        {
            string filePath = Path.Combine( _options.OutputDirectory, $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss}.json" );

            using FileStream stream = File.OpenWrite( filePath );

            await JsonSerializer.SerializeAsync( stream, animes, _serializerOptions, token );
        }

        public void Dispose()
        {
            _httpClient.Dispose();
            GC.SuppressFinalize( this );
        }

        public class DownloaderConfiguration
        {
            public string MALClientId { get; set; } = string.Empty;
            public string MALUserName { get; set; } = string.Empty;
            public int Retry { get; set; }
            public string OutputDirectory { get; set; } = string.Empty;
        }
    }
}
