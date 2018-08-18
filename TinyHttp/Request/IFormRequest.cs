﻿using System.Collections.Generic;

namespace Tiny.Http
{
    /// <summary>
    /// Interface IFormRequest
    /// </summary>
    /// <seealso cref="Tiny.Http.ICommonResquest" />
    /// <seealso cref="Tiny.Http.IExecutableRequest" />
    public interface IFormRequest : ICommonResquest, IExecutableRequest
    {
        /// <summary>
        /// Adds the form parameter.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The current request</returns>
        IFormRequest AddFormParameter(string key, string value);

        /// <summary>
        /// Adds the form parameters.
        /// </summary>
        /// <param name="datas">The datas.</param>
        /// <returns>The current request</returns>
        IFormRequest AddFormParameters(IEnumerable<KeyValuePair<string, string>> datas);
    }
}