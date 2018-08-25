﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Tiny.Http
{
    /// <summary>
    /// Class TinyRequest.
    /// </summary>
    /// <seealso cref="Tiny.Http.IRequest" />
    /// <seealso cref="Tiny.Http.IOctectStreamRequest" />
    /// <seealso cref="Tiny.Http.IStreamRequest" />
    internal class TinyRequest : IRequest, IOctectStreamRequest, IStreamRequest, IMultiPartFromDataRequest, IMultiPartFromDataExecutableRequest
    {
        private readonly HttpVerb _httpVerb;
        private readonly TinyHttpClient _client;
        private readonly string _route;
        private Dictionary<string, string> _headers;
        private Dictionary<string, string> _queryParameters;
        private ITinyContent _content;
        private List<KeyValuePair<string, string>> _formParameters;
        private MultiPartContent _multiPartFormData;

        internal HttpVerb HttpVerb { get => _httpVerb; }
        internal Dictionary<string, string> QueryParameters { get => _queryParameters; }
        internal string Route { get => _route; }
        internal ITinyContent Content { get => _content; }

        internal TinyRequest(HttpVerb httpVerb, string route, TinyHttpClient client)
        {
            _httpVerb = httpVerb;
            _route = route;
            _client = client;
            _headers = new Dictionary<string, string>();
        }

        #region Content
        public IContentRequest AddContent<TContent>(TContent content, ISerializer serializer)
        {
            _content = new ToSerializeContent<TContent>(content, serializer);
            return this;
        }

        public IContentRequest AddByteArrayContent(byte[] byteArray, string contentType)
        {
            _content = new BytesContent(byteArray, contentType);
            return this;
        }

        public IContentRequest AddStreamContent(Stream stream, string contentType)
        {
            _content = new TinyStreamContent(stream, contentType);
            return this;
        }

        #endregion

        #region Forms Parameters

        /// <summary>
        /// Adds the form parameter.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>IFormRequest.</returns>
        public IFormRequest AddFormParameter(string key, string value)
        {
            if (_formParameters == null)
            {
                _formParameters = new List<KeyValuePair<string, string>>();
                _content = new FormParametersContent(_formParameters, null);
            }

            _formParameters.Add(new KeyValuePair<string, string>(key, value));
            return this;
        }

        /// <summary>
        /// Adds the form parameters.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns>IFormRequest.</returns>
        public IFormRequest AddFormParameters(IEnumerable<KeyValuePair<string, string>> items)
        {
            if (_formParameters == null)
            {
                _formParameters = new List<KeyValuePair<string, string>>();
                _content = new FormParametersContent(_formParameters, null);
            }

            _formParameters.AddRange(items);
            return this;
        }
        #endregion

        #region Headers

        /// <summary>
        /// Adds the header.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The current request</returns>
        public IRequest AddHeader(string key, string value)
        {
            if (_headers == null)
            {
                _headers = new Dictionary<string, string>();
            }

            _headers.Add(key, value);
            return this;
        }
        #endregion

        #region Query Parameters

        /// <summary>
        /// Adds the query parameter.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The current request</returns>
        public IRequest AddQueryParameter(string key, string value)
        {
            if (_queryParameters == null)
            {
                _queryParameters = new Dictionary<string, string>();
            }

            if (!_queryParameters.ContainsKey(key))
            {
                _queryParameters.Add(key, value);
            }
            else
            {
                // TODO : Throw an exception ?
                _queryParameters[key] = value;
            }

            return this;
        }

        /// <summary>
        /// Adds the query parameter.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The current request</returns>
        public IRequest AddQueryParameter(string key, int value)
        {
            return AddQueryParameter(key, value.ToString());
        }

        /// <summary>
        /// Adds the query parameter.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The current request</returns>
        public IRequest AddQueryParameter(string key, uint value)
        {
            return AddQueryParameter(key, value.ToString());
        }

        /// <summary>
        /// Adds the query parameter.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The current request</returns>
        public IRequest AddQueryParameter(string key, double value)
        {
            return AddQueryParameter(key, value.ToString());
        }

        /// <summary>
        /// Adds the query parameter.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The current request</returns>
        public IRequest AddQueryParameter(string key, decimal value)
        {
            return AddQueryParameter(key, value.ToString());
        }
        #endregion

        /// <summary>
        /// Withes the byte array response.
        /// </summary>
        /// <returns>IOctectStreamRequest.</returns>
        public IOctectStreamRequest WithByteArrayResponse()
        {
            return this;
        }

        /// <summary>
        /// Withes the stream response.
        /// </summary>
        /// <returns>IStreamRequest.</returns>
        public IStreamRequest WithStreamResponse()
        {
            return this;
        }

        public Task<TResult> ExecuteAsync<TResult>(IDeserializer deserializer, CancellationToken cancellationToken)
        {
            return _client.ExecuteAsync<TResult>(this, deserializer, cancellationToken);
        }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return _client.ExecuteAsync(this, cancellationToken);
        }

        Task<byte[]> IOctectStreamRequest.ExecuteAsync(CancellationToken cancellationToken)
        {
            return _client.ExecuteByteArrayResultAsync(this, cancellationToken);
        }

        Task<Stream> IStreamRequest.ExecuteAsync(CancellationToken cancellationToken)
        {
            return _client.ExecuteWithStreamResultAsync(this, cancellationToken);
        }

        #region MultiPart

        public IMultiPartFromDataRequest AsMultiPartFromDataRequest(string contentType)
        {
            _multiPartFormData = new MultiPartContent(contentType);
            _content = _multiPartFormData;
            return this;
        }

        IMultiPartFromDataRequest IMultiPartFromDataRequest.AddByteArray(byte[] data, string name, string fileName, string contentType)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            _multiPartFormData.Add(new BytesMultiPartData(data, name, fileName, contentType));

            return this;
        }

        IMultiPartFromDataRequest IMultiPartFromDataRequest.AddStream(Stream data, string name, string fileName, string contentType)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            _multiPartFormData.Add(new StreamMultiPartData(data, name, fileName, contentType));

            return this;
        }

        IMultiPartFromDataExecutableRequest IMultiPartFromDataExecutableRequest.AddByteArray(byte[] data, string name, string fileName, string contentType)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            _multiPartFormData.Add(new BytesMultiPartData(data, name, fileName, contentType));

            return this;
        }

        IMultiPartFromDataExecutableRequest IMultiPartFromDataExecutableRequest.AddStream(Stream data, string name, string fileName, string contentType)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            _multiPartFormData.Add(new StreamMultiPartData(data, name, fileName, contentType));

            return this;
        }
        #endregion
    }
}