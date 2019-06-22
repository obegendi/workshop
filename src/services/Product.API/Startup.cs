using Autofac;
using Autofac.Extensions.DependencyInjection;
using CacheLayer;
using Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Product.API.Infrastructure.ApiClient;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Product.API
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, false)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            var minWorkerThreads = Configuration.GetValue<int>("App:MinWorkerThreads");
            var minIOThreads = Configuration.GetValue<int>("App:MinIOThreads");
            ThreadPool.SetMinThreads(minWorkerThreads, minIOThreads);
        }
        public IConfiguration Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services
                .AddSingleton(_ => Configuration)
                .AddHttpClients(Configuration)
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            var hcBuilder = services.AddHealthChecks();
            hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy());
            services.AddMQ(Configuration);
            services.AddSwaggerGen(options =>
            {
                options.DescribeAllEnumsAsStrings();
                options.SwaggerDoc("v1", new Info
                {
                    Title = "Product API",
                    Version = "v1",
                    Description = "The Product Service API",
                    TermsOfService = "Terms Of Service"
                });
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            
            var container = new ContainerBuilder();
            container.Populate(services);
            return new AutofacServiceProvider(container.Build());
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCustomLogger(env, Configuration);
            Log.ForContext<Startup>()
                .Information("<{EventID:l}> Configure Started {Application} with {@configuration}",
                    "Startup", env.ApplicationName, Configuration);
            LazyRedis.InitializeConnectionString($"{Configuration.GetConnectionString("Redis")},abortConnect=false");
            LazyRedis.DefaultDb = int.Parse(Configuration.GetConnectionString("RedisDb"));

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseSwagger();
            app.UseSwaggerUI(x =>
            {
                x.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                x.RoutePrefix = string.Empty;
            });
            
            app.UseMvc();
        }
    }
}
