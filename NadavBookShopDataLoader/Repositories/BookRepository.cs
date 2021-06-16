using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using NadavBookShopDataLoader.Interface;
using NadavBookShopDataLoader.Models;
using Newtonsoft.Json;

namespace NadavBookShopDataLoader
{
    public class BookRepository : IBooksRepository
    {
        string SOURCE_PATH_KEY = "SourcePath";
        string TARGET_PATH_KEY = "TargetPath";
        private string _configPath;
        string[] _skipAuthors;
        Dictionary<string, string> _configDic = new Dictionary<string, string>();

        public BookRepository()
        {
            _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NadavBookShopDataLoader", "BookLoaderConfig.json");
            _skipAuthors = new string[] { "peter" };
            GetDataFromConfig();
        }

        #region SourceTarget Config
    
        /// <summary>
        /// Will show to the user the source and target directory and give the option to change
        /// </summary>
        private void SetSourceTargetDir()
        {
            string updatedFinalMessage = "";
            Console.Write($"Do you want to change the SOURCE directory ? Source directory path {_configDic[SOURCE_PATH_KEY] } ? [Yes/No]");

            string answer = Console.ReadLine();
            if (answer.ToLower().StartsWith("y"))
            {
                while (!Directory.Exists(answer))
                {
                    Console.Write($"Please enter a valid source directory {Environment.NewLine}");
                    answer = Console.ReadLine();
                }
                _configDic[SOURCE_PATH_KEY] = answer;
                updatedFinalMessage += $"Source directory Updated {Environment.NewLine} ";
            }

            Console.Write($"Do you want to change the TARGET directory ? target directory path {_configDic[SOURCE_PATH_KEY] } ? [Yes/No]");
            answer = Console.ReadLine();
           
            if (answer.ToLower().StartsWith("y"))
            {
                while (!Directory.Exists(answer))
                {
                    Console.Write($"Please enter a valid target directory {Environment.NewLine}");
                    answer = Console.ReadLine();
                }
                _configDic[TARGET_PATH_KEY] = answer;
                updatedFinalMessage += $"Target directory Updated {Environment.NewLine} ";
            }

            string json = JsonConvert.SerializeObject(_configDic, Formatting.Indented);
            File.WriteAllText(_configPath, json);

            Console.Write(updatedFinalMessage + "\n");
        }

        /// <summary>
        /// Will get source Directory and target directory from a json config file
        /// if the file does not exist, will create a new one and set the source and target the running directory
        /// </summary>
        private void GetDataFromConfig()
        {
            if (!File.Exists(_configPath))
            {
                _configDic.Add(SOURCE_PATH_KEY, Directory.GetCurrentDirectory());
                _configDic.Add(TARGET_PATH_KEY, Directory.GetCurrentDirectory());

                string json = JsonConvert.SerializeObject(_configDic, Formatting.Indented);

                if (!Directory.Exists(Path.GetDirectoryName(_configPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_configPath));
                }
                File.WriteAllText(_configPath, json);
            }
            else
            {
                string configText = File.ReadAllText(_configPath);

                if (configText == string.Empty)
                {
                    throw new FileNotFoundException($"Missing {_configPath}");
                }

                _configDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(configText);

                if (!_configDic.ContainsKey(SOURCE_PATH_KEY))
                {
                    throw new InvalidDataException($"{Path.GetFileName(_configPath)} file is missing {SOURCE_PATH_KEY}");
                }
                if (!_configDic.ContainsKey(TARGET_PATH_KEY))
                {
                    throw new InvalidDataException($"{Path.GetFileName(_configPath)} file is missing {TARGET_PATH_KEY}");
                }

            }

        }

        #endregion SourceTarget Config
    
        /// <summary>
        /// Will get data from ALL the JSon files in SOURCE_PATH
        /// </summary>
        /// <returns></returns>
        public List<Book> GetNewBooks()
        {
            SetSourceTargetDir();

            string[] ArrFilesToLoad = Directory.GetFiles(_configDic[SOURCE_PATH_KEY], "*.json");
            List<Book> newBookList = new List<Book>();

            if (ArrFilesToLoad.Length == 0)
            {
                throw new FieldAccessException($"The system did not find a Json file, please check that exist a file of type Json in {_configDic[SOURCE_PATH_KEY]}");
            }

            foreach (string filePath in ArrFilesToLoad)
            {
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

            return newBookList;

        }

        /// <summary>
        /// A valid book is a book that was not published on Saturday
        /// and the Author's name does not include the word 'Peter' (or any other word in ArrSkipAuthors)
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        public bool IsBookValidToSell(Book book)
        {
            bool isValid = DateTime.Parse(book.publish_date).DayOfWeek != DayOfWeek.Saturday;

            foreach (string skipAutor in _skipAuthors)
            {
                if (book.author.ToLower().Contains(skipAutor))
                {
                    isValid = false;
                    break;
                }
            }


            return isValid;
        }

        /// <summary>
        /// will check that all the properties of the Book have data.
        /// That the publish_date is a valid Date
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public string CheckBookDataIsValid(Book book)
        {
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
            return errorMessage;
        }

        public string RoundPrice(decimal price)
        {
            return Math.Ceiling(price).ToString("$#");
        }

        public string LoadDataToStore(List<Book> bookList)
        {
            string targetPath = Path.Combine(_configDic[TARGET_PATH_KEY], $"NewBooksInStoreFile_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.csv");

            if (!Directory.Exists(_configDic[TARGET_PATH_KEY]))
            {
                Directory.CreateDirectory(_configDic[TARGET_PATH_KEY]);
            }

            using (var writer = new StreamWriter(targetPath))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(bookList);
                }
            }

            return targetPath;

        }

    }
}
