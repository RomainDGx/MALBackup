using CK.Core;
using System;
using System.IO;
using System.Text;

namespace MALBackup.App
{
    /// <summary>
    /// Allws logs to be written to a text file and logs to be captured before the actual fil path is knwon.
    /// This implementation MUST NOT to be generalized (ie. moved to CK.ActivityMonitor package) since the dispose Must be explictly
    /// handled (there is more work to be done to have a correct implementation of this).
    /// CKSetup handles this correctly but outside of it, it would be too dangerous.
    /// </summary>
    public class BufferedFileMonitorClient : ActivityMonitorTextWriterClient, IDisposable
    {
        StringBuilder _buffer;
        StreamWriter _fileWriter;

        BufferedFileMonitorClient( LogFilter filter, StreamWriter w )
            : base( filter )
        {
            if( w == null )
            {
                _buffer = new StringBuilder();
                Writer = s => _buffer.Append( s );
            }
            else
            {
                _fileWriter = w;
                Writer = w.Write;
            }
        }

        /// <summary>
        /// Factory method.
        /// </summary>
        /// <param name="m">The monitor to use. Must not be null.</param>
        /// <param name="logFilePath">The log path: when not null, a 'CKSetup.log' is created and no buffering is required.</param>
        /// <param name="filter">Optional filter to apply. Defaults to <see cref="LogFilter.Undefined"/>.</param>
        /// <returns>The registered client.</returns>
        public static BufferedFileMonitorClient RegisterClient( IActivityMonitor m, string logFilePath = null, LogFilter filter = default )
        {
            var c = logFilePath != null
                        ? new BufferedFileMonitorClient( filter, CreateFileWriter( m, logFilePath ) )
                        : new BufferedFileMonitorClient( filter, null );
            m.Output.RegisterClient( c );
            return c;
        }

        /// <summary>
        /// Flushes and closes the file.
        /// </summary>
        public void Dispose()
        {
            Writer = null;
            if( _fileWriter != null )
            {
                _fileWriter.Flush();
                _fileWriter.Dispose();
                _fileWriter = null;
            }
        }

        /// <summary>
        /// When the log path was not initally known, this method can be called (only once).
        /// </summary>
        /// <param name="m">The monitor to use.</param>
        /// <param name="logFilePath">Te log file path.</param>
        public void SetFile( IActivityMonitor m, string logFilePath )
        {
            if( _buffer == null )
            {
                throw new InvalidOperationException( "SetFile must be called only once." );
            }
            if( (_fileWriter = CreateFileWriter( m, logFilePath )) != null )
            {
                var captured = _buffer.ToString();
                _fileWriter.Write( captured );
                m.Debug( $"BufferedFileMonitorClient wrote {captured.Length} bytes before SetFile( '{logFilePath}' )." );
                _buffer = null;
                Writer = _fileWriter.Write;
            }
        }

        static StreamWriter CreateFileWriter( IActivityMonitor m, string logFilePath )
        {
            if( String.IsNullOrWhiteSpace( logFilePath ) ) throw new ArgumentNullException( nameof( logFilePath ) );
            try
            {
                logFilePath = Path.GetFullPath( logFilePath );
                string dir = Path.GetDirectoryName( logFilePath );
                Directory.CreateDirectory( dir );
                return new StreamWriter( logFilePath, true, Encoding.UTF8, 4096 );
            }
            catch( Exception ex )
            {
                m.Warn( $"While creating log file: {logFilePath}", ex );
                return null;
            }
        }
    }
}
