
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;

namespace NadavBookShopDataLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            ILogger<BookLoader> logger = serviceProvider.GetService<ILogger<BookLoader>>();

            BookLoader bookLoader = new BookLoader(new BookRepository(), logger);
            bookLoader.LoadBooksToStore();
        }


        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole())
            .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information)
            .AddTransient<BookLoader>();
        }
    }
}
