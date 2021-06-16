
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace NadavBookShopDataLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            //create Loogger
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            ILogger<BookLoader> logger = serviceProvider.GetService<ILogger<BookLoader>>();
            ILogger<BookRepository> BookRepositorylogger = serviceProvider.GetService<ILogger<BookRepository>>();

            BookLoader bookLoader = new BookLoader(new BookRepository(BookRepositorylogger), logger);
            bookLoader.LoadBooksToStore();
        }


        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddNLog("nlog.config");
            });
        }
    }
}
