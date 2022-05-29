using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FileBaseDirectoryWatcher_01
{
    internal class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to FileBase Sync");

            var host = CreateDefaultBuilder().Build();
            
            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;
            var workerInstance = provider.GetRequiredService<FileWatcher>();

            IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .AddUserSecrets<Program>()
            .Build();

            Console.WriteLine(@$"FileBaseDirectory: {config["FileBaseDirectory"]}");

            //workerInstance.Execute();
            //var fw = new FileWatcher(config);
            //fw.Execute();

            host.Run();
            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        static IHostBuilder CreateDefaultBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(app =>
                {
                    app.AddJsonFile("appsettings.json");
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<FileWatcher>();
                });
        }

        //static private void ConfigureServices(IServiceCollection serviceCollection)
        //{
        //    //ILoggerFactory loggerFactory = new Logging.LoggerFactory();
        //    //serviceCollection.AddInstance<ILoggerFactory>(loggerFactory);
        //    //var svcCol = new ServiceCollection();
        //   // IServiceProvider provider = svcs.BuildServiceProvider();
        //    //var runtime = provider.GetRequiredService<RunTime>();


        //    var config = new ConfigurationBuilder()
        //                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
        //                    .AddJsonFile("appsettings.json")
        //                    .AddUserSecrets<Program>()
        //                    .Build();

        //    Console.WriteLine(@$"Credentials: accessKey: {config["FileBaseDirectory"]}");

        //}

    }
}
