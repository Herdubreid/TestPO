using BlazorState;
using Celin.PO;
using McMaster.Extensions.CommandLineUtils;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

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

                var server = new Celin.AIS.Server(config["baseUrl"]);
                server.AuthRequest.username = "DEMO";
                server.AuthRequest.password = "DEMO";

                var services = new ServiceCollection()
                    .AddSingleton(server)
                    .AddBlazorState
                        ((options) => options.Assemblies = new Assembly[]
                        { typeof(Program).GetTypeInfo().Assembly })
                    .AddScoped<POState>()
                    .AddScoped<Mediator>()
                    .AddMediatR(typeof(POState))
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
