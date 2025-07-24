namespace ReverseProxy
{
    /// <summary>Extensions for <see cref="HttpResponse"/>.</summary>
    public static class HttpResponseExtensions
    {
        /// <summary>Copies the content of an <see cref="HttpResponseMessage"/> to an <see cref="HttpResponse"/>.</summary>
        /// <param name="response">The <see cref="HttpResponse"/> to copy the content to. Must not be null.</param>
        /// <param name="httpResponseMessage">The <see cref="HttpResponseMessage"/> to copy the content from. Must not be null.</param>
        /// <returns>A task that represents the asynchronous copy operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="response"/> or <paramref name="httpResponseMessage"/> is null.</exception>
        public static async Task CopyFromAsync(this HttpResponse response, HttpResponseMessage httpResponseMessage)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));
            if (httpResponseMessage == null) throw new ArgumentNullException(nameof(httpResponseMessage));

            // 1. Copy Status Code
            response.StatusCode = (int)httpResponseMessage.StatusCode;

            // 2. Copy response headers (excluding headers that shouldn't be copied)
            var headersToSkip = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "transfer-encoding", "connection", "upgrade", "proxy-connection"
            };
            
            foreach (var header in httpResponseMessage.Headers)
            {
                if (!headersToSkip.Contains(header.Key))
                {
                    response.Headers[header.Key] = header.Value.ToArray();
                }
            }

            // 3. Copy content headers (this was missing in the original!)
            if (httpResponseMessage.Content != null)
            {
                foreach (var header in httpResponseMessage.Content.Headers)
                {
                    if (!headersToSkip.Contains(header.Key))
                    {
                        response.Headers[header.Key] = header.Value.ToArray();
                    }
                }

                // 4. Copy response content efficiently
                await httpResponseMessage.Content.CopyToAsync(response.Body);
            }
        }
    }
}
