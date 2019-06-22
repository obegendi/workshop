using System;
using System.Threading;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using DataAccessLayer.Contexts.Repository;
using Extensions;
using MessageQueueProvider;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.Swagger;
using Write.API.Infrastructure.EventHandlers;
using Write.API.Infrastructure.Events;

namespace Write.API
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.Configure<Settings>(options =>
            {
                options.ConnectionString
                    = Configuration.GetSection("MongoConnection:ConnectionString").Value;
                options.Database
                    = Configuration.GetSection("MongoConnection:Database").Value;
            });
            var hcBuilder = services.AddHealthChecks();
            hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy());
            services.AddMQ(Configuration);
            services.AddTransient<NewProductAwaitingIntegrationEventHandler>();
            services.AddTransient<ProductRepository>();

            services.AddSwaggerGen(options =>
            {
                options.DescribeAllEnumsAsStrings();
                options.SwaggerDoc("v1", new Info
                {
                    Title = "Write API",
                    Version = "v1",
                    Description = "The Write Service API",
                    TermsOfService = "Terms Of Service"
                });
            });

            var container = new ContainerBuilder();
            container.Populate(services);

            return new AutofacServiceProvider(container.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseSwagger();
            app.UseSwaggerUI(x =>
            {
                x.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                x.RoutePrefix = string.Empty;
            });
            app.UseCustomLogger(env, Configuration);
            app.UseMvc();
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
            eventBus.Subscribe<NewProductAwaitingIntegrationEvent, NewProductAwaitingIntegrationEventHandler>();

        }
    }
}
