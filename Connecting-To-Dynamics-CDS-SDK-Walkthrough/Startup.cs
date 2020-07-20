using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Cds.Client;
using System;

[assembly: FunctionsStartup(typeof(MyNamespace.Startup))]

namespace MyNamespace
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton((cdsServiceClient) => {
                // build connection string
                string dynamicsEnvironmentUrl = Environment.GetEnvironmentVariable("DynamicsEnvironmentUrl");
                string dynamicsClientId = Environment.GetEnvironmentVariable("DynamicsClientId");
                string dynamicsClientSecret = Environment.GetEnvironmentVariable("DynamicsClientSecret");
                string connectionString = $@"AuthType=ClientSecret;Url={dynamicsEnvironmentUrl};ClientId={dynamicsClientId};ClientSecret={dynamicsClientSecret}";
                // connect to dynamics
                CdsServiceClient service = new CdsServiceClient(connectionString);
                Console.WriteLine("Connected to the CDS");
                return service;
            });
        }
    }
}