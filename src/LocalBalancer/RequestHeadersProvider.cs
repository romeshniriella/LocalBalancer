using System;
using System.Collections.Generic;
using System.Net.Http;
using LocalBalancer.Abstractions;

namespace LocalBalancer
{
    public class RequestHeadersProvider : IRequestHeadersProvider
    {
        public virtual void SetHeaders(NetworkNodeSettings node, HttpClient client)
        {
            foreach (KeyValuePair<string, string> header in node.Headers)
            {
                if (client.DefaultRequestHeaders.Contains(header.Key))
                {
                    throw new ArgumentException($"Header {header.Key} already exists");
                }

                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }
    }
}