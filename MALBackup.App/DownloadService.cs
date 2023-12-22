using CK.Core;
using MALBackup.Model;
using Microsoft.Extensions.Options;
using Quartz;
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
    public class DownloadService : IJob
    {
        static readonly JsonSerializerOptions _serializerOptions = new()
        {
            WriteIndented = true,
            Converters = { StatusConverter.Instance }
        };

        readonly IActivityMonitor _monitor;
        readonly IOptions<DownloaderConfiguration> _options;

        public DownloadService( IActivityMonitor monitor, IOptions<DownloaderConfiguration> options )
        {
            _monitor = monitor;
            _options = options;
        }

        public async Task Execute( IJobExecutionContext context )
        {
            using var client = new HttpClient
            {
                DefaultRequestHeaders = { { "X-MAL-CLIENT-ID", _options.Value.MALClientId } },
            };

            var animes = new List<Anime>();

            string? requestUri = $"https://api.myanimelist.net/v2/users/{_options.Value.MALUserName}/animelist?sort=anime_title&limit=1000&offset=0&nsfw=true&fields=id,title,num_episodes,list_status{{status,num_episodes_watched,num_times_rewatched,rewatch_value}}";

            for( int tryDownload = 1; tryDownload <= _options.Value.Retry; tryDownload++ )
            {
                _monitor.Info( $"Downloading anime list, attempt number {tryDownload}." );

                try
                {
                    while( requestUri is not null && !context.CancellationToken.IsCancellationRequested )
                    {
                        using var response = await client.GetAsync( requestUri, context.CancellationToken );
                        await EnsureResponseAsync( response, context.CancellationToken );

                        var content = await JsonSerializer.DeserializeAsync<ResponsePayload>( await response.Content.ReadAsStreamAsync( context.CancellationToken ),
                                                                                              _serializerOptions,
                                                                                              context.CancellationToken );
                        content = Check( content );
                        animes.AddRange( Check( content.Data ).Select( node =>
                        {
                            Check( node.Anime ).UserStatus = Check( node.UserListStatus );
                            return node.Anime!;
                        } ) );
                        requestUri = Check( content.Paging ).Next;
                    }
                }
                catch( MALApiResponseException e )
                {
                    var builder = new StringBuilder()
                        .AppendLine( e.Message )
                        .Append( "Status code: " )
                        .AppendLine( e.StatusCode.ToString() )
                        .Append( "Response content: " )
                        .AppendLine( e.Response?.ToString() );

                    _monitor.Error( builder.ToString(), e );
                    await Task.Delay( TimeSpan.FromSeconds( 5 ) );
                }
                catch( Exception e )
                {
                    _monitor.Error( e );
                    await Task.Delay( TimeSpan.FromSeconds( 5 ) );
                }

                if( requestUri is null ) break;
            }

            if( requestUri is null )
            {
                await SaveAnimesAsync( animes, context.CancellationToken );
                _monitor.Info( "THe operation was successfully competed." );
            }
            else
            {
                _monitor.Info( "The operation was unsuccessful." );
            }
        }

        static async Task EnsureResponseAsync( HttpResponseMessage response, CancellationToken cancellationToken )
        {
            if( !response.IsSuccessStatusCode )
            {
                var errorResponse = await JsonSerializer.DeserializeAsync<ErrorResponse>( await response.Content.ReadAsStreamAsync( cancellationToken ),
                                                                                          _serializerOptions,
                                                                                          cancellationToken );

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
        async Task SaveAnimesAsync( List<Anime> animes, CancellationToken cancellationToken )
        {
            string filePath = Path.Combine( _options.Value.OutputDirectory, $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss}.json" );

            using FileStream stream = File.OpenWrite( filePath );

            await JsonSerializer.SerializeAsync( stream, animes, _serializerOptions, cancellationToken );
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
