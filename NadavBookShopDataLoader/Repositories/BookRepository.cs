using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using Microsoft.Extensions.Logging;
using NadavBookShopDataLoader.Interface;
using NadavBookShopDataLoader.Models;
using Newtonsoft.Json;

namespace NadavBookShopDataLoader
{
    public class BookRepository : IBooksRepository
    {
        #region Variables
        string SOURCE_PATH_KEY = "SourcePath";
        string TARGET_PATH_KEY = "TargetPath";
        private string _configPath;
        string[] _skipAuthors;
        Dictionary<string, string> _configDic = new Dictionary<string, string>();
        ILogger<BookRepository> _logger;

        #endregion Variables

        #region constructor
        public BookRepository(ILogger<BookRepository> logger)
        {
            _logger = logger;
            _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NadavBookShopDataLoader", "BookLoaderConfig.json");
            _skipAuthors = new string[] { "peter" };
            GetDataFromConfig();
        }

        #endregion constructor

        #region SourceTarget Config

        /// <summary>
        /// Will show to the user the source and target directory and give the option to change
        /// </summary>
        private void SetSourceTargetDir()
        {
            _logger.LogDebug("BookRepository.SetSourceTargetDir(START)");

            string updatedFinalMessage = "";
            Console.Write($"Do you want to change the SOURCE directory ? Source directory path {_configDic[SOURCE_PATH_KEY] } ? [Yes/No]");
            string sourceDirectory = string.Empty;
            string targetDirectory = string.Empty;

            string answer = Console.ReadLine();
            if (answer.ToLower().StartsWith("y"))
            {
                while (!Directory.Exists(sourceDirectory))
                {
                    Console.Write($"Please enter a valid source directory {Environment.NewLine}");
                    sourceDirectory = Console.ReadLine();
                }
                updatedFinalMessage += $"Source directory Updated {Environment.NewLine} ";
            }

            Console.Write($"Do you want to change the TARGET directory ? target directory path {_configDic[SOURCE_PATH_KEY] } ? [Yes/No]");
            answer = Console.ReadLine();

            if (answer.ToLower().StartsWith("y"))
            {
                while (!Directory.Exists(targetDirectory))
                {
                    Console.Write($"Please enter a valid target directory {Environment.NewLine}");
                    targetDirectory = Console.ReadLine();
                }
                updatedFinalMessage += $"Target directory Updated {Environment.NewLine} ";
            }

            if (updatedFinalMessage != string.Empty)
            {
                _logger.LogInformation(updatedFinalMessage);
                CreateConfigFile(sourceDirectory, targetDirectory);
            }



            _logger.LogDebug("BookRepository.SetSourceTargetDir(END)");
        }

        /// <summary>
        /// Will get source Directory and target directory from a json config file
        /// if the file does not exist, will create a new one and set the source and target the running directory
        /// </summary>
        private void GetDataFromConfig()
        {
            _logger.LogDebug("BookRepository.GetDataFromConfig(START)");
            if (File.Exists(_configPath))
            {
                string configText = File.ReadAllText(_configPath);

                if (configText == string.Empty)
                {
                    throw new FileNotFoundException($"Missing {_configPath}");
                }

                _configDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(configText);

                //if the _configDic is missing a SourcePath ot TargetPath, or the Directories dont exist, create default _config
                if (!_configDic.ContainsKey(SOURCE_PATH_KEY) || !_configDic.ContainsKey(TARGET_PATH_KEY)
                    || !Directory.Exists(_configDic[SOURCE_PATH_KEY]) || !Directory.Exists(_configDic[TARGET_PATH_KEY]))
                {
                    CreateConfigFile(Directory.GetCurrentDirectory(), Directory.GetCurrentDirectory());
                }
            }
            else
            {
                CreateConfigFile(Directory.GetCurrentDirectory(), Directory.GetCurrentDirectory());
            }
            _logger.LogDebug("BookRepository.GetDataFromConfig(END)");
        }

        private void CreateConfigFile(string sourceDirectory, string targetDirectory)
        {
            _configDic.Clear();
            _configDic.Add(SOURCE_PATH_KEY, sourceDirectory);
            _configDic.Add(TARGET_PATH_KEY, targetDirectory);

            string json = JsonConvert.SerializeObject(_configDic, Formatting.Indented);

            if (!Directory.Exists(Path.GetDirectoryName(_configPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_configPath));
            }
            File.WriteAllText(_configPath, json);

            _logger.LogInformation($"Source directory = {sourceDirectory} {Environment.NewLine}" +
                                   $"Target directory = {targetDirectory}");
        }
        #endregion SourceTarget Config

        /// <summary>
        /// Will get data from ALL the JSon files in _configDic[SOURCE_PATH_KEY]
        /// </summary>
        /// <returns></returns>
        public List<Book> GetNewBooksFromSource()
        {
            _logger.LogDebug($"BookRepository.GetNewBooksFromSource(START)");

            SetSourceTargetDir();

            string[] FilesToLoad = Directory.GetFiles(_configDic[SOURCE_PATH_KEY], "*.json");
            List<Book> newBookList = new List<Book>();

            if (FilesToLoad.Length == 0)
            {
                throw new FieldAccessException($"The system did not find a Json file, please check that exist a file of type Json in {_configDic[SOURCE_PATH_KEY]}");
            }
            _logger.LogInformation($"Exist {FilesToLoad.Length} json files to load to store" );

            foreach (string filePath in FilesToLoad)
            {
                try
                {
                    //do try/catch because if there is more than one file and one of them is valid, I want to process it
                    string allFileText = File.ReadAllText(filePath);

                    Library newLibrary = JsonConvert.DeserializeObject<Library>(allFileText);

                    if (newLibrary == null)
                        throw new FormatException($"json file '{Path.GetFileName(filePath)}' is not correct");

                    if (newLibrary.catalog == null)
                        throw new FormatException($"json file '{Path.GetFileName(filePath)}' is not correct, missing Catalog");

                    if (newLibrary.catalog.book == null)
                        throw new FormatException($"json file '{Path.GetFileName(filePath)}' is not correct, missing Book[]");

                    newBookList.AddRange(newLibrary.catalog.book);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Errors trying to get data from source File {Path.GetFileName(filePath)}, ex= {ex.Message}");
                }
            }

            _logger.LogDebug($"BookRepository.GetNewBooksFromSource(END)");
            return newBookList;

        }

        /// <summary>
        /// A valid book is a book that was not published on Saturday
        /// and the Author's name does not include the word 'Peter' (or any other word in _skipAuthors)
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        public bool IsBookValidToSell(Book book)
        {
            _logger.LogDebug($"BookRepository.IsBookValidToSell(START)[book.Id= {book.Id}]");

            bool isValid = DateTime.Parse(book.publish_date).DayOfWeek != DayOfWeek.Saturday;

            foreach (string skipAutor in _skipAuthors)
            {
                if (book.author.ToLower().Contains(skipAutor))
                {
                    isValid = false;
                    break;
                }
            }

            _logger.LogDebug($"BookRepository.IsBookValidToSell(END)[book.Id= {book.Id}]");

            return isValid;
        }

        /// <summary>
        /// will check that all the properties of the Book have data.
        /// That the publish_date is a valid Date
        /// That the price is a valid decimal
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        public string CheckBookDataIsValid(Book book)
        {
            _logger.LogDebug($"BookRepository.CheckBookDataIsValid(START)[book.Id= {book.Id}]");
            string baseErrorMessage = $"value of '{0}' is empty or invalid";
            string errorMessage = "";

            if (book.Id == string.Empty)
            {
                errorMessage = $"Book from author='{book.author}' with title= '{book.title}' is missing and 'id'";
            }
            else if (book.publish_date == string.Empty)
            {
                errorMessage = string.Format(baseErrorMessage, "publish_date");
            }
            else if (book.price == string.Empty)
            {
                errorMessage = string.Format(baseErrorMessage, "price");
            }

            else if (book.title == string.Empty)
            {
                errorMessage = string.Format(baseErrorMessage, "title");
            }
            else if (book.genre == string.Empty)
            {
                errorMessage = string.Format(baseErrorMessage, "genre");
            }
            else if (book.description == string.Empty)
            {
                errorMessage = string.Format(baseErrorMessage, "description");
            }
            else if (book.author == string.Empty)
            {
                errorMessage = string.Format(baseErrorMessage, "author");
            }

            if (errorMessage == string.Empty)
            {
                //check that piblish_date is a valid date
                DateTime publishDate;

                if (DateTime.TryParse(book.publish_date, out publishDate) == false)
                {
                    errorMessage = $"Publish date is not a valid Date";
                }
            }
            if (errorMessage == string.Empty)
            {
                //check that price is a valid decimal
                decimal price;

                if (decimal.TryParse(book.price, out price) == false)
                {
                    errorMessage = $"Price is not a valid Decimal";
                }
                else
                {
                    book.price = RoundPrice(price);
                }
            }
            _logger.LogDebug($"BookRepository.CheckBookDataIsValid(END)[book.Id= {book.Id}]");
            return errorMessage;
        }

        public string RoundPrice(decimal price)
        {
            return Math.Ceiling(price).ToString("$#");
        }

        public string LoadDataToStore(List<Book> bookList)
        {
            _logger.LogDebug($"BookRepository.LoadDataToStore(START)[bookList.count = {bookList.Count}]");
            string targetPath = Path.Combine(_configDic[TARGET_PATH_KEY], $"NewBooksInStoreFile_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.csv");

            if (!Directory.Exists(_configDic[TARGET_PATH_KEY]))
            {
                Directory.CreateDirectory(_configDic[TARGET_PATH_KEY]);
                _logger.LogInformation($"Create new directory, path ={_configDic[TARGET_PATH_KEY]}");
            }

            using (var writer = new StreamWriter(targetPath))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(bookList);
                }
            }

            _logger.LogDebug($"BookRepository.LoadDataToStore(END)[bookList.count = {bookList.Count}]");
            return targetPath;

        }

    }
}
