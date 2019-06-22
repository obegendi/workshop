using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace Extensions
{
    public static class AspNetExtensions
    {
        public static IApplicationBuilder UseCustomLogger(this IApplicationBuilder app, IHostingEnvironment env, IConfiguration configuration)
        {
            var level = configuration.GetValue<LogEventLevel>("Serilog:LogEventLevel");
            var elasticsearchEndpoint = configuration.GetValue<string>("Serilog:ElasticsearchEndpoint");
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", env.ApplicationName)
                .Enrich.WithProperty("Environment", env.EnvironmentName)
                .MinimumLevel.Override("Microsoft", level)
                .MinimumLevel.Override("System", level)
                .MinimumLevel.Override("System.Net.Http.HttpClient", level)
                .WriteTo.Elasticsearch(
                    new ElasticsearchSinkOptions(
                        new Uri(elasticsearchEndpoint))
                    {
                        AutoRegisterTemplate = true,
                        ConnectionTimeout = TimeSpan.FromMilliseconds(5000),
                        MinimumLogEventLevel = level,
                        TemplateName = "serilog-events-template",
                        IndexFormat = "log-{0:yyyy.MM.dd}"
                    })
                .MinimumLevel.Is(level)
                .CreateLogger();
            return app;
        }
    }
}
