
using Microsoft.Extensions.Logging;
using NadavBookShopDataLoader.Interface;
using NadavBookShopDataLoader.Interfaces;
using NadavBookShopDataLoader.Models;
using NadavBookShopDataLoader.Validators;

using System;
using System.Collections.Generic;


namespace NadavBookShopDataLoader
{
    internal class BookLoader
    {
        #region Variables
        private readonly ILogger _logger;
        public IGetBooksRepository _getBooksRepository;
        public ISaveBooksRepository _saveBooksRepository;

        List<IValidate> _bookValidationsList;
        #endregion Variables

        #region constructor
        public BookLoader(IGetBooksRepository getBooksRepository, ISaveBooksRepository saveBooksRepository
                        , List<IValidate> bookValidationsList, ILogger logger)
        {
            _bookValidationsList = bookValidationsList;
            _getBooksRepository = getBooksRepository;
            _saveBooksRepository = saveBooksRepository;
            _logger = logger;
        }

        #endregion constructor

        /// <summary>
        /// Will get New Books and load to store
        /// </summary>
        public void ProcessBooksFile()
        {
            try
            {
                _logger.LogDebug("BookLoader.LoadBooksToStore(START)");

                List<Book> allBook = _getBooksRepository.GetNewBooksFromSource();
                List<Book> newValidBooks = new List<Book>();
                List<Book> invalidBooks = new List<Book>();

                _logger.LogInformation($"Got {allBook.Count} new Books");

                foreach (Book book in allBook)
                {
                    try
                    {
                        //Do try/catch because want to continue loading books 

                        foreach (IValidate validation in _bookValidationsList)
                        {
                            validation.IsBookValid(book);
                        }

                        newValidBooks.Add(book);
                    }
                    catch (BookValidationException ex)
                    {
                        _logger.LogError($"Error Processing book with id='{book.Id}', Property {ex.InvalidProperty} ex = {ex.Message}");
                        invalidBooks.Add(book);  //to make file with books that where not saved
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error Processing book with id='{book.Id}', ex= {ex.Message} ");
                        invalidBooks.Add(book);
                    }
                }
                _logger.LogInformation($"{newValidBooks.Count} from {allBook.Count} books in file, are valid and will be load to store");

                if (newValidBooks.Count > 0)
                {
                    string targetPath = _saveBooksRepository.SaveBooks(newValidBooks,
                                                            ConfigFile.ConfigDict[ConfigFile.TARGET_PATH_KEY],
                                                            $"NewBooksInStoreFile_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.csv");
                    _logger.LogInformation($"csv file was saved: {targetPath}");
                }
                if (invalidBooks.Count > 0)
                {
                    _saveBooksRepository.SaveBooks(invalidBooks,
                                          ConfigFile.ConfigDict[ConfigFile.TARGET_PATH_KEY],
                                          $"InvalidBooksInFile_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.csv");
                    _logger.LogInformation($"csv file with invalid books was saved: {ConfigFile.ConfigDict[ConfigFile.TARGET_PATH_KEY]}");
                }
                _logger.LogDebug("BookLoader.LoadBooksToStore(END)");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Critical Error Loading new Books to the store, Ex: {ex.ToString()}");
            }
        }
    }
}
