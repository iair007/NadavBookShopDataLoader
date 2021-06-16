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
    public class SaveBooksRepository : ISaveBooksRepository
    {
        #region Variables
       
        ILogger _logger;

        #endregion Variables

        #region constructor
        public SaveBooksRepository(ILogger logger)
        {
            _logger = logger;
        }

        #endregion constructor

        public string SaveBooks(List<Book> bookList, string targetDir, string fileName)
        {
            try
            {
                _logger.LogDebug($"BookRepository.LoadDataToStore(START)[bookList.count = {bookList.Count}]");
                string targetPath = Path.Combine(targetDir,fileName );

                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                    _logger.LogInformation($"Create new directory, path ={targetDir}");
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
            catch (Exception ex) {
                _logger.LogError($"Error saving Csv file, ex= {ex.Message}");
                throw ex;
            }
        }

    }
}
