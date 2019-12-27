using Blazor.Extensions.Storage.Interfaces;
using BlazorState;
using Celin.PO;
using McMaster.Extensions.CommandLineUtils;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace TestPO
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                IConfiguration config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", false, true)
                    .Build();

                // Initialise the Logger
                var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder
                        .SetMinimumLevel(LogLevel.Critical)
                        .AddConsole();
                });
                ILogger logger = loggerFactory.CreateLogger<Program>();

                var server = new Celin.AIS.Server(config["baseUrl"], logger);
                server.AuthRequest.username = "DEMO";
                server.AuthRequest.password = "DEMO";

                var services = new ServiceCollection()
                    .AddLogging(config => {
                        config
                        .SetMinimumLevel(LogLevel.None)
                        .AddConsole();
                    })
                    .AddScoped<Mediator>()
                    .AddScoped<ILocalStorage, FileStorage>()
                    .AddBlazorState
                        ((options) => options.Assemblies = new Assembly[]
                        { typeof(Program).GetTypeInfo().Assembly })
                    .AddScoped<POState>()
                    .AddMediatR(typeof(POState))
                    .AddSingleton(server)
                    .BuildServiceProvider();

                var app = new CommandLineApplication<Cmd>();
                app.Conventions
                    .UseDefaultConventions()
                    .UseConstructorInjection(services);

                app.Execute(args);
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: {e.Message}");
            }
        }
    }
}
