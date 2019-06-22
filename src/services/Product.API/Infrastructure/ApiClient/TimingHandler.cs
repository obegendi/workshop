using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Product.API.Infrastructure.ApiClient
{
    public class TimingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            Log.ForContext<TimingHandler>().Information("Starting request");
            Log.ForContext<TimingHandler>().Debug("{type} {headers} {httpMethod} {uri} {content}", "request", request?.Headers?.ToString(),
                request?.Method?.Method, request?.RequestUri, request?.Content?.ReadAsStringAsync()?.Result);
            var response = await base.SendAsync(request, cancellationToken);
            Log.ForContext<TimingHandler>().Debug("{type} {headers} {statusCode} {content}", "response", response?.Content?.Headers?.ToString(),
                response?.StatusCode, response?.Content?.ReadAsStringAsync()?.Result);
            Log.ForContext<TimingHandler>().Information($"Finished request in {sw.ElapsedMilliseconds}ms");

            return response;
        }
    }
}
