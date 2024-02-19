using CK.Core;
using CK.Monitoring.Handlers;
using CK.Monitoring;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace MALBackup.App
{
    class Program
    {
        static async Task Main( string[] args )
        {
            var config = LoadConfiguration( args );

            using var go = InitGrandOutput( config );
            var monitor = new ActivityMonitor( "Main monitor" );

            using var downloader = Downloader.TryCreate( monitor, config );
            if( downloader is null ) return;

            await downloader.ExecuteAync();
        }

        static IConfigurationRoot LoadConfiguration( string[] args )
        {
            return new ConfigurationBuilder()
                .SetBasePath( Directory.GetCurrentDirectory() )
                .AddJsonFile( "appsettings.json", optional: false, reloadOnChange: true )
#if DEBUG
                .AddJsonFile( "appsettings.Development.json", optional: true, reloadOnChange: true )
#endif
                .AddEnvironmentVariables()
                .AddCommandLine( args )
                .Build();
        }

        static GrandOutput? InitGrandOutput( IConfigurationRoot config )
        {
            if( config["LogDirectory"] is not string logDirectory )
            {
                System.Console.WriteLine( "The 'LogDirectory' property is missing in the configuration." );
                return null;
            }
            var goConfig = new GrandOutputConfiguration { Handlers = { new TextFileConfiguration { Path = logDirectory } } };

            if( bool.TryParse( config["UseConsoleLog"], out bool useConsoleLog ) && useConsoleLog )
            {
                goConfig.Handlers.Add( new ConsoleConfiguration() );
            }
            return GrandOutput.EnsureActiveDefault( goConfig );
        }
    }
}
