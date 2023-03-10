using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using WebApplication3.AsyncDataServices;
using WebApplication3.AsyncDataServices.SignalR;
using WebApplication3.Data;
using WebApplication3.SyncDataServices.Grpc;
using WebApplication3.SyncDataServices.http;

namespace WebApplication3
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (_env.IsDevelopment())
            {
                Console.WriteLine("Using InMem database");
                services.AddDbContext<AppDBContext>(opt => opt.UseInMemoryDatabase("TestPlatformDBInMemory"));
            }

            if (_env.IsProduction())
            {
                Console.WriteLine("Using MSSQL server");
                services.AddDbContext<AppDBContext>(opt =>
                    opt.UseSqlServer(Configuration.GetConnectionString("PlatformsConn")));
            }

            services.AddSignalR();

            services.AddScoped<IPlatformRepo, PlatformRepo>();

            services.AddHttpClient<ICommandDataClients, HttpCommandDataClients>();
            services.AddSingleton<IMessageBusClient, MessageBusClient>();
            
            services.AddGrpc();

            services.AddControllers();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApplication3", Version = "v1" });
            });

            services.AddRouting(options => options.LowercaseUrls = true);

            Console.WriteLine($"--> CommandService Endpoint {Configuration["CommandServiceUrl"]}");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApplication3 v1"));
            }

            app.Use(async (context, next) =>
            {
                Console.WriteLine($"Method: {context.Request.Method}");
                Console.WriteLine($"Path: {context.Request.Path.Value}");
                await next();
            });
            
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGrpcService<GrpcPlatformService>();

                endpoints.MapHub<ServerHub>("/serverHubWebSocket", options =>
                {
                    options.Transports = HttpTransportType.WebSockets;
                });
                
                endpoints.MapHub<ServerHub>("/serverHubSSE", options =>
                {
                    options.Transports = HttpTransportType.ServerSentEvents;
                });
                
                endpoints.MapHub<ServerHub>("/serverHubLongPooling", options =>
                {
                    options.Transports = HttpTransportType.LongPolling;
                });

                endpoints.MapGet("/protos/platforms.proto", async context =>
                {
                    await context.Response.WriteAsync(File.ReadAllText("Protos/platforms.proto"));
                });
            });

            PrepDB.PrepPopulation(app, _env.IsProduction());
        }
    }
}