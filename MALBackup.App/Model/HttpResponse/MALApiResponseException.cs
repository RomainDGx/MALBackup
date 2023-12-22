using System;
using System.Net;

namespace MALBackup.App
{
    /// <summary>
    /// Error throws in the MyAnimeList API requests/response context.
    /// </summary>
    public class MALApiResponseException : Exception
    {
        /// <param name="statusCode">Status code of the http response.</param>
        /// <param name="response">Content of the http response.</param>
        /// <param name="message">Message that describes the error.</param>
        public MALApiResponseException( HttpStatusCode statusCode, ErrorResponse? response, string? message = null )
            : base( message )
        {
            StatusCode = statusCode;
            Response = response;
        }

        /// <summary>
        /// Status code of the http response.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Content of the http response.
        /// </summary>
        public ErrorResponse? Response { get; }
    }
}
