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

            // 3. Copy content headers and body (but not for responses that must not have content)
            if (httpResponseMessage.Content != null)
            {
                foreach (var header in httpResponseMessage.Content.Headers)
                {
                    if (!headersToSkip.Contains(header.Key))
                    {
                        response.Headers[header.Key] = header.Value.ToArray();
                    }
                }

                // 4. Copy response content efficiently, but skip for responses that must not have content
                if (ShouldNotHaveContent(httpResponseMessage.StatusCode))
                {
                    return;
                }
                await httpResponseMessage.Content.CopyToAsync(response.Body);
            }
        }

        /// <summary>
        /// Determines if an HTTP status code should have a message body.
        /// Per HTTP specs: 1xx, 204, 205, and 304 responses MUST NOT have a message body
        /// </summary>
        /// <param name="statusCode">The HTTP status code to check.</param>
        /// <returns>True if the response should have content, false otherwise.</returns>
        private static bool ShouldNotHaveContent(System.Net.HttpStatusCode statusCode)
        {
            var code = (int)statusCode;
            
            // Return false for status codes that MUST NOT have a message body:
            // 1xx Informational, 204 No Content, 205 Reset Content, 304 Not Modified
            return (code >= 100 && code <= 199) ||
                    statusCode == System.Net.HttpStatusCode.NoContent ||
                    statusCode == System.Net.HttpStatusCode.ResetContent ||
                    statusCode == System.Net.HttpStatusCode.NotModified;
        }
    }
}
