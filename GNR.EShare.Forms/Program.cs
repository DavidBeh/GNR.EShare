using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Composite;
using Microsoft.Extensions.FileProviders.Internal;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;

namespace GNR.EShare.Forms
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static async Task Main()
        {
            ApplicationConfiguration.Initialize();

            var builder = WebApplication.CreateBuilder();

            builder.WebHost.UseKestrel(options =>
            {
                options.Listen(IPAddress.Any, 0);

            });

            builder.Services.AddSingleton<SharedFileProviderService>();
            builder.Services.AddControllers();
            builder.Services.AddSingleton<PortProvider>();
            builder.Services.AddHostedService<PortProvider>(provider => provider.GetRequiredService<PortProvider>());
            builder.Services.AddTransient<Form1>();
            var app = builder.Build();

            app.MapControllers();
            await app.StartAsync();

            var form = app.Services.GetRequiredService<Form1>();


            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            Application.Run(form);
        }


    }

}