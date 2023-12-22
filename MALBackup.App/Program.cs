using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using System;

namespace MALBackup.App
{
    class Program
    {
        static void Main( string[] args )
        {
            Host.CreateDefaultBuilder( args )
                .UseCKMonitoring()
                .ConfigureServices( ( ctx, services ) =>
                {
                    services.Configure<DownloadService.DownloaderConfiguration>( config =>
                    {
                        string? malClientId = ctx.Configuration["MALClientId"];
                        if( string.IsNullOrEmpty( malClientId ) ) throw new Exception( "Configuration parameter 'MALClientId' is missing." );
                        config.MALClientId = malClientId;

                        string? malUserName = ctx.Configuration["MALUserName"];
                        if( string.IsNullOrEmpty( malUserName ) ) throw new Exception( "Configuration parameter 'MALUserName' is missing." );
                        config.MALUserName = malUserName;

                        string? strRetry = ctx.Configuration["Retry"];
                        if( !int.TryParse( strRetry, out int retry ) ) throw new Exception( "Configuration parameter 'Retry' is missing." );
                        config.Retry = retry;

                        string? outputDirectory = ctx.Configuration["OutputDirectory"];
                        if( string.IsNullOrEmpty( outputDirectory ) ) throw new Exception( "Configuration parameter 'OutputDirectory' is missing." );
                        config.OutputDirectory = outputDirectory;
                    } );

                    services.AddQuartz( config =>
                    {
                        var jobKey = new JobKey( "DownloadAnimeJob" );
                        config.AddJob<DownloadService>( c => c.WithIdentity( jobKey ) );

                        config.AddTrigger( c =>
                        {
                            string? cronExpression = ctx.Configuration["CronExpression"];
                            if( string.IsNullOrEmpty( cronExpression ) ) throw new Exception( "Configuration parameter 'CronExpression' is missing." );
                            c.ForJob( jobKey )
                             .WithIdentity( "DownloadAnimeJob-trigger" )
                             .WithCronSchedule( cronExpression );
                        } );
                    } )
                    .AddQuartzHostedService( c => c.WaitForJobsToComplete = true );
                } )
                .Build()
                .Run();
        }
    }
}
