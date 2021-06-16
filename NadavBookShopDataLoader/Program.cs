
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NadavBookShopDataLoader.Interfaces;
using NadavBookShopDataLoader.Validators;
using NLog;
using NLog.Extensions.Logging;
using System.Collections.Generic;

namespace NadavBookShopDataLoader
{
    class Program
    {
        static List<IValidate> _validationsList = new List<IValidate>();
        static void Main(string[] args)
        {
            //create Loogger
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            ILogger<BookLoader> bookLoaderLogger = serviceProvider.GetService<ILogger<BookLoader>>();
            ILogger<GetBooksRepository> getBooksRepositorylogger = serviceProvider.GetService<ILogger<GetBooksRepository>>();
            ILogger<SaveBooksRepository> saveBooksRepositorylogger = serviceProvider.GetService<ILogger<SaveBooksRepository>>();
            
            SetValidationsList();

            BookLoader bookLoader = new BookLoader(new GetBooksRepository(getBooksRepositorylogger),
                                  new SaveBooksRepository(saveBooksRepositorylogger)
                                , _validationsList, bookLoaderLogger);
            bookLoader.ProcessBooksFile();
        }

        private static void SetValidationsList()
        {
            _validationsList.Add(new ValidateBookData());
            _validationsList.Add(new ValidateBookGenre());
            _validationsList.Add(new ValidateBooksAuthor());
            _validationsList.Add(new ValidateBookPrice());
            _validationsList.Add(new ValidateBookPublishDate());
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
                builder.AddNLog("nlog.config");
            });
        }
    }
}
