#### Local Balancer

In a decentralized world we mostly work with external network endpoints which can break at any time. 
Scaled internal applications can also break at any time.
To increase the reliability and availability of a service, with an always-on experience, this nuget was born.
At the heart of this library we use Polly as our resiliency mechanisms provider. 
This library uses retry policies and One circuit breaker per node. 

This nuget supports any type of networking client and are rich with extension points.

#### What’s supported
- Standby nodes are now supported. 
- Per node headers are also supported (ID headers, Auth headers etc). 
- Ability to prevent retrying on requests are also supported. 
- Using a client other than HttpClient is also supported. ie: SocketClient, RestClient etc.
- You can have your own primary-node, and backups without worrying about switching between them when a request fails on one.

##### Requirements
- netstandard2.1
- VsCode or Visual Studio
- Multiple downstream services that can serve the same request

##### Usage
1. Add the node configuration to the configuration store. 

It is recommended to save the authentication keys in a Secret Store (Azure key vault for example).  
Following is an excerpt from `appsettings.Development.json`

```json
{
  ....
  // more config settings
  ....
  "BalancerConfiguration": {
    "CircuitBreakDurationMinutes" : 1,
    "CircuitBreakThreadhold" : 2,
    "RetryCount" : 3,
    "Nodes": {
      "MainNet": {
        "Headers": {
          "X-API-KEY": "7c6a9e99500e4a448753548d5ddecb9b"
        },
        "IsActive": true,
        "NetworkAddress": "https://primary.network.address"
      },
      "Backup": {
        "Headers": {},
        "IsActive": true,
        "NetworkAddress": "https://secondary.network.address",
      }
    }
  },
  ....
  // more config settings
  ....
}
```

2. Add DI support for multiple nodes by calling the extension method `AddBalancer<TResult>(config)`

There are many other extension methods that you can use to extend the features. Minimal setup lokks like:

```cs
public static IServiceCollection ConfigureService(this IServiceCollection services, IConfiguration configuration)
{
    return services
        .AddBalancer<HttpResponseMessage>(configuration) //<-- Add this line, Here
        .Configure<MyServiceSettings>(configuration.GetSection(nameof(MyServiceSettings)));
}
```
 
3. Use `IBalancedNetworkPolicy<TResult>` for retry capabilities

```cs

        private readonly IBalancedNetworkPolicy<RpcResponseMessage> _nodePolicy;

        //...initialization code

        protected override Task<RpcResponseMessage> SendAsync(RpcRequestMessage request, string route = null)
        {
            // prevent retries on requests that might update some state (eg: POST/PUT requests)
            if (this.ShouldSkipRetry(request, route))
            {
                return this.SendRequestAsync(request, route);
            }

            // execute the multi node policy on other request types (ie: GET)
            return _nodePolicy.ExecuteAsync(
                   // which will create a new rpc client every time we do a retry
                   node => this.SendRequestAsync(request, route),
            // : provides cancellation capabilities to the policy
            default);
        }

```

4. In the method you invoke the requests, retrieve an `HttpClient` from  `IBalancedHttpClientProvider` instance.

This instance can be disposed of once the operation is complete.

```cs
        private readonly IBalancedHttpClientProvider _provider;

        //...initialization code

        private async Task<RpcResponseMessage> SendRequestAsync(RpcRequestMessage request, string route = null)
        {
            try
            {
                var cancellationTokenSource = new CancellationTokenSource();

                // http client factory returns a new HttpClient everytime we request one
                // while reusing handlers from a pool.
                // therefore it is safer to dispose after we've done the requests
                using HttpClient httpClient = _provider.CreateClient();
                
                string rpcRequestJson = JsonConvert.SerializeObject(request, _jsonSerializerSettings);
                using var httpContent = new StringContent(rpcRequestJson, Encoding.UTF8, "application/json");

                cancellationTokenSource.CancelAfter(ConnectionTimeout);

                _logger.LogInformation(rpcRequestJson);

                // will retry on multiple nodes internally when you do Post()
                HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(route, httpContent, cancellationTokenSource.Token).ConfigureAwait(false);

                httpResponseMessage.EnsureSuccessStatusCode();

                Stream stream = await httpResponseMessage.Content.ReadAsStreamAsync();
                using (var streamReader = new StreamReader(stream))
                using (var reader = new JsonTextReader(streamReader))
                {
                    var serializer = JsonSerializer.Create(_jsonSerializerSettings);
                    RpcResponseMessage message = serializer.Deserialize<RpcResponseMessage>(reader);

                    _logger.LogInformation("{@RpcResponseMessage}", message);

                    return message;
                }
            }
            catch (TaskCanceledException ex)
            {
                var exception = new HttpRequestException($"Rpc timeout after {ConnectionTimeout.TotalMilliseconds} milliseconds", ex);
                _logger.LogError(exception, ex.Message);

                throw exception;
            }
            catch (Exception ex)
            {
                var exception = new HttpRequestException("Error occurred when trying to send rpc request: " + request.Method, ex);
                _logger.LogError(exception, ex.Message);

                throw exception;
            }
        }
```

#### Retry Policy Configuration Options

- `Retry Count` - The number of retries per request. Default is 3.
- `Circuit Break Threadhold` - The number of exceptions that are allowed before opening the circuit. Default is 2.
- `Circuit Break Duration in Minutes` - The duration the circuit will stay open before resetting. Default is 1.

#### Node Configuration Options
- `Headers` - a list of key value pairs. Used for auth headers and trace headers. extension point is `IRequestHeadersProvider`
- `IsActive` - is the node active? inactive nodes will not be used. Ideal to skip a node temporarily.
- `NetworkAddress` - the base address of the node. Must be a valid URI
- `Priority` - Nodes will be sorted by their priority and the lowest value is the highest priority

#### Extension Points

Use the DI registration overloads of `AddBalancer` for this purpose.

```
 public static IServiceCollection AddBalancer<THttpResult, TClientProvider, TAuthProvider>(
            this IServiceCollection services,
            IConfiguration configuration)
            where TClientProvider : IBalancedHttpClientProvider
            where TAuthProvider : class, IRequestHeadersProvider
            where TMetricsProvider : class, IMetricsTracker
```

where

- `THttpResult` is the result type you get from node. eg: RpcResponseMessage, HttpResponseMessage, MyOtherServiceResponse etc
- `TClientProvider` - an implementation of `IBalancedHttpClientProvider` to customize a transport channel
- `TAuthProvider` - an implementation of `IRequestHeadersProvider` to provide custom authentication for your transport channel
- `TMetricsProvider` - an implementation of `IMetricsTracker` to track balancer metrics

#### Future Work
- Latency metrics
- Better retries
  - Capture specific errors that are OK to retry – make it configurable
- Emit Resiliency Metrics – to answer the questions such as 
  - Number of retries done per minute per node
  - Number of broken nodes
  - Mean time to recovery (MTTR)
  - Downstream node error types
- Automatic node disengagement by periodically checking node-health
