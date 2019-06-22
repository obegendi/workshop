using System;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Product.API.Infrastructure.ApiClient
{
    public static class MiddlewareExtensions
    {
        public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ISearchApiClient, SearchApiClient>();
            services.AddSingleton<IReadApiClient, ReadApiClient>();
            services.AddTransient<TimingHandler>();
            var searchApi = configuration.GetValue<string>("Search.API");
            var readApi = configuration.GetValue<string>("Read.API");
            Log.Debug("search.api => {api}", searchApi);
            Log.Debug("read.api => {api}", readApi);

            services.AddHttpClient("SearchAPI", client =>
            {
                client.BaseAddress = new Uri(searchApi);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("User-Agent", "Product.API");
            }).AddHttpMessageHandler<TimingHandler>();

            services.AddHttpClient("ReadAPI", client =>
            {
                client.BaseAddress = new Uri(readApi);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("User-Agent", "Product.API");
            }).AddHttpMessageHandler<TimingHandler>();

            return services;
        }
    }
}
