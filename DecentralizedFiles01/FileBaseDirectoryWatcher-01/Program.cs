using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using FileBaseSync;
using FileBaseSync.Services;
using FileBaseSync.Models;

namespace FileBaseDirectoryWatcher_01
{
    internal class Program
    {
        static private FileWatcher fileWatcher;

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to FileBase Sync");

            var host = CreateDefaultBuilder().Build();

            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;
            fileWatcher = provider.GetRequiredService<FileWatcher>();

            host.Run();
            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        static IHostBuilder CreateDefaultBuilder()
        {
            var hostBuilder = Host.CreateDefaultBuilder();
            IConfiguration config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .AddUserSecrets<Program>()
                    .Build();

            hostBuilder.ConfigureAppConfiguration(app =>
               {
                   app.AddJsonFile("appsettings.json");
                    //ToDo: Note - Each developer needs to change this
                   app.AddUserSecrets("75b24535-8edf-454c-9b21-ae1e0ea0f89c");
                   app.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                   .AddJsonFile("appsettings.json")
                   .AddUserSecrets<Program>();
                   //.Build();

                   app.AddConfiguration(config);
                   

               })
               .ConfigureServices(services =>
               {
                    //services.AddSingleton<IConfiguration>();
                   services.AddSingleton<FileWatcher>();
                   services.AddSingleton<FileBaseService>();
                   services.AddSingleton<FileBaseDataBroker>();
                   services.AddSingleton<LocalFileService>();
                   services.AddSingleton<LocalFileDataBroker>();
                   services.AddSingleton<FileBaseSyncService>();

                   services.Configure<FileBaseCredentialsOptions>(config.GetSection("FilebaseCredentials"));

                   services.AddOptions<FileBaseCredentialsOptions>()
                    .Bind(config.GetSection(FileBaseCredentialsOptions.Credentials));

               });

            Console.WriteLine(@$"Watching FileBase Directory: {config["FileBaseDirectory"]}");
            return hostBuilder;
        }

    }
}
