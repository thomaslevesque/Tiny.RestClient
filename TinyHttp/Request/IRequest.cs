﻿using System.IO;

namespace Tiny.Http
{
    /// <summary>
    /// Interface IRequest
    /// </summary>
    /// <seealso cref="Tiny.Http.IFormRequest" />
    /// <seealso cref="Tiny.Http.IExecutableRequest" />
    public interface IRequest : IExecutableRequest, IFormRequest
    {
        /// <summary>
        /// Adds the content.
        /// </summary>
        /// <typeparam name="TContent">The type of the t content.</typeparam>
        /// <param name="content">The content.</param>
        /// <param name="serializer">Override the default serializer setted on the client.</param>
        /// <returns>The current request</returns>
        IParameterRequest AddContent<TContent>(TContent content, IFormatter serializer = null);

        /// <summary>
        /// Adds the content of the byte array.
        /// </summary>
        /// <param name="byteArray">The byte array.</param>
        /// <param name="contentType">The Content type</param>
        /// <returns>The current request</returns>
        IParameterRequest AddByteArrayContent(byte[] byteArray, string contentType = "application/octet-stream");

        /// <summary>
        /// Adds the content of the stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="contentType">The Content type</param>
        /// <returns>The current request</returns>
        IParameterRequest AddStreamContent(Stream stream, string contentType = "application/octet-stream");

        /// <summary>
        /// As a multipart data from request
        /// </summary>
        /// <param name="contentType">content type of the request (default value  = "multipart/form-data")</param>
        /// <returns>The current request</returns>
        IMultiPartFromDataRequest AsMultiPartFromDataRequest(string contentType = "multipart/form-data");
    }
}