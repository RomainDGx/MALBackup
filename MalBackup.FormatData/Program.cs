using MALBackup.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MalBackup.FormatData
{
    internal class Program
    {
        static JsonSerializerOptions _options = new()
        {
            WriteIndented = true
        };

        static async Task Main( string[] args )
        {
            string inputPath = CheckValidDirectoryPath( args[0] );
            string outputPath = CheckValidDirectoryPath( args[1] );

            Console.WriteLine( "Input path '{0}'.", inputPath );
            Console.WriteLine( "Output path '{0}'.", outputPath );

            foreach( string filePath in Directory.GetFiles( inputPath, "*.json" ) )
            {
                var animes = (await GetFileDataAsync( filePath )).Select( oldAnime => oldAnime.ToAnime() );

                string fileName = Path.GetFileName( filePath );
                await SaveDataAsync( animes, Path.Combine( outputPath, fileName ) );

                Console.WriteLine( "File {0} done.", fileName );
            }
        }

        static string CheckValidDirectoryPath( string path )
        {
            if( !Directory.Exists( path ) )
            {
                throw new ArgumentException( $"Invalid directory path '{path}'." );
            }
            return path;
        }

        static async Task<IEnumerable<OldAnimeFormat>> GetFileDataAsync( string filePath )
        {
            using FileStream stream = File.OpenRead( filePath );

            var animes = await JsonSerializer.DeserializeAsync<IEnumerable<OldAnimeFormat>>( stream );
            if( animes is null )
            {
                throw new InvalidDataException( $"Empty anime list in '{filePath}'" );
            }
            return animes;
        }

        static async Task SaveDataAsync( IEnumerable<Anime> animes, string fileName )
        {
            using FileStream stream = File.OpenWrite( fileName );
            await JsonSerializer.SerializeAsync( stream, animes, _options );
        }
    }
}
