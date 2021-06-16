
using Microsoft.Extensions.Logging;
using NadavBookShopDataLoader.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NadavBookShopDataLoader
{
    internal class BookLoader
    {
        #region Variables
        private readonly ILogger _logger;
        public BookRepository _repository { get; }
        #endregion Variables

        #region constructor
        public BookLoader(BookRepository repository, ILogger<BookLoader> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        #endregion constructor

        /// <summary>
        /// Will get New Books and load to store
        /// </summary>
        public void LoadBooksToStore()
        {
            try
            {
                List<Book> allBook = _repository.GetNewBooksFromSource();
                List<Book> newValidBooks = new List<Book>();

                _logger.LogInformation($"Got {allBook.Count} new Books");

                foreach (Book book in allBook)
                {
                    try
                    {
                        //Do try/catch because want to continue loading books 

                        string errorMessage = _repository.CheckBookDataIsValid(book);
                        if (errorMessage != string.Empty)
                        {
                            throw new FormatException(errorMessage);
                        }

                        if (_repository.IsBookValidToSell(book))
                        {
                            newValidBooks.Add(book);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error Processing book with id='{book.Id}', ex= {ex.Message} ");
                    }

                }
                _logger.LogInformation($"{newValidBooks.Count} from {allBook.Count} books in file, are valid and will be load to store");

                if (newValidBooks.Count > 0)
                {
                    string targetPath = _repository.LoadDataToStore(newValidBooks);
                    _logger.LogInformation($"csv file was saved: {targetPath}");

                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Critical Error Loading new Books to the store, Ex: {ex.ToString()}");
            }
        }
    }
}
