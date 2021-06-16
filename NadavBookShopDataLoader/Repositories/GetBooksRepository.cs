using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using NadavBookShopDataLoader.Interface;
using NadavBookShopDataLoader.Models;
using NadavBookShopDataLoader.Validators;
using Newtonsoft.Json;


namespace NadavBookShopDataLoader
{
    public class GetBooksRepository : IGetBooksRepository
    {
        #region Variables

        private string _configPath;
        ILogger _logger;

        #endregion Variables

        #region constructor
        public GetBooksRepository(ILogger logger)
        {
            _logger = logger;
            _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NadavBookShopDataLoader", "BookLoaderConfig.json");
            SetConfigDict();
        }

        #endregion constructor

        #region Source and Target Config

        /// <summary>
        /// Show to the user the source and target directory and give the option to change
        /// </summary>
        private void UserUpdateConfig()
        {
            _logger.LogDebug("BookRepository.UserUpdateConfig(START)");

            Console.Write($"Welcome to Nadav's Book Shop Data Loader {Environment.NewLine}");
            string mainMessage = $"**************************************************************** {Environment.NewLine}" +
                                 $"This is your configuration, to update something insert the corresponding number {Environment.NewLine}" +
                                 $"1- Json file's path (Source)= {ConfigFile.ConfigDict[ConfigFile.SOURCE_PATH_KEY]} {Environment.NewLine}" +
                                 $"2- JSon files's Name = {ConfigFile.ConfigDict[ConfigFile.SOURCE_FILE_NAME_KEY]} {Environment.NewLine}" +
                                 $"3- Csv file's path (Target)= {ConfigFile.ConfigDict[ConfigFile.TARGET_PATH_KEY]} {Environment.NewLine}" +
                                 $"4- Start {Environment.NewLine}" +
                                 $"****************************************************************" + 
                                 $"{Environment.NewLine}";

            Console.Write(mainMessage);
            string answer = Console.ReadLine();

            while (answer.Trim() != "4")
            {

                switch (answer.Trim())
                {
                    case "1":
                        string sourceDirectory = string.Empty;
                        while (!Directory.Exists(sourceDirectory))
                        {
                            Console.Write($"Please enter a valid source directory {Environment.NewLine}");
                            sourceDirectory = Console.ReadLine();
                        }

                        UpsertConfig(ConfigFile.SOURCE_PATH_KEY, sourceDirectory);

                        break;
                    case "2":
                        string sourceFileName = string.Empty;

                        Console.Write($"Inser a file name with extension 'json' {Environment.NewLine}");
                        sourceFileName = Console.ReadLine();
                        string filePath = Path.Combine(ConfigFile.ConfigDict[ConfigFile.SOURCE_PATH_KEY], sourceFileName);
                        while (!File.Exists(filePath))
                        {
                            Console.Write($"File does not exist in source Path, Please enter a valid name {Environment.NewLine}");
                            sourceFileName = Console.ReadLine();
                            filePath = Path.Combine(ConfigFile.ConfigDict[ConfigFile.SOURCE_PATH_KEY], sourceFileName);
                        }
                        while (Path.GetExtension(filePath).ToLower() != ".json")
                        {
                            Console.Write($"File need to be a JSon file, Please enter a valid name {Environment.NewLine}");
                            sourceFileName = Console.ReadLine();
                        }

                        UpsertConfig(ConfigFile.SOURCE_FILE_NAME_KEY, sourceFileName);

                        break;
                    case "3":
                        string targetDirectory = string.Empty;
                        while (!Directory.Exists(targetDirectory))
                        {
                            Console.Write($"Please enter a valid target directory {Environment.NewLine}");
                            targetDirectory = Console.ReadLine();
                        }

                        UpsertConfig(ConfigFile.TARGET_PATH_KEY, targetDirectory);

                        break;
                    case "4":

                        break;
                    default:
                        Console.Write("Invalid Input, please select one of the valid options 1,2,3,4");
                        break;
                }
                Console.Write(mainMessage);
                answer = Console.ReadLine();

            }

            _logger.LogDebug("BookRepository.UserUpdateConfig(END)");
        }

        /// <summary>
        /// Will get source Directory and target directory from a json config file
        /// if the file does not exist, will create a new one and set the source and target the running directory
        /// </summary>
        private void SetConfigDict()
        {
            _logger.LogDebug("BookRepository.GetDataFromConfig(START)");
            if (File.Exists(_configPath))
            {
                string configText = File.ReadAllText(_configPath);

                if (configText == string.Empty)
                {
                    throw new FileNotFoundException($"Missing {_configPath}");
                }

                ConfigFile.ConfigDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(configText);

                //if the _configDic is missing a SourcePath ot TargetPath, or the Directories dont exist, create default _config
                if (!ConfigFile.ConfigDict.ContainsKey(ConfigFile.SOURCE_PATH_KEY) || !Directory.Exists(ConfigFile.ConfigDict[ConfigFile.SOURCE_PATH_KEY]))
                {
                    UpsertConfig(ConfigFile.SOURCE_PATH_KEY, Directory.GetCurrentDirectory());
                }
                if (!ConfigFile.ConfigDict.ContainsKey(ConfigFile.TARGET_PATH_KEY) || !Directory.Exists(ConfigFile.ConfigDict[ConfigFile.TARGET_PATH_KEY]))
                {
                    UpsertConfig(ConfigFile.TARGET_PATH_KEY, Directory.GetCurrentDirectory());
                }
            }
            else
            {
                //the config file does not exist, create a new one
                UpsertConfig(ConfigFile.SOURCE_PATH_KEY, Directory.GetCurrentDirectory());
                UpsertConfig(ConfigFile.SOURCE_FILE_NAME_KEY, ConfigFile.SOURCE_FILE_NAME);
                UpsertConfig(ConfigFile.TARGET_PATH_KEY, Directory.GetCurrentDirectory());
            }
            _logger.LogDebug("BookRepository.GetDataFromConfig(END)");
        }

        private void UpsertConfig(string key, string value)
        {
            if (ConfigFile.ConfigDict.ContainsKey(key))
            {
                ConfigFile.ConfigDict[key] = value;
            }
            else
            {
                ConfigFile.ConfigDict.Add(key, value);
            }

            string json = JsonConvert.SerializeObject(ConfigFile.ConfigDict, Formatting.Indented);

            if (!Directory.Exists(Path.GetDirectoryName(_configPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_configPath));
            }
            File.WriteAllText(_configPath, json);

            _logger.LogInformation($"{key} was Upsert into the _configDict");
        }
        #endregion SourceTarget Config

        /// <summary>
        /// Will get data from ALL the JSon files in _configDic[SOURCE_PATH_KEY]
        /// </summary>
        /// <returns></returns>
        public List<Book> GetNewBooksFromSource()
        {
            _logger.LogDebug($"BookRepository.GetNewBooksFromSource(START)");

            UserUpdateConfig();

            string[] FilesToLoad = Directory.GetFiles(ConfigFile.ConfigDict[ConfigFile.SOURCE_PATH_KEY], ConfigFile.ConfigDict[ConfigFile.SOURCE_FILE_NAME_KEY]);
            List<Book> newBookList = new List<Book>();

            if (FilesToLoad.Length == 0)
            {
                throw new Exception($"The system did not find a Json file, please check that exist a file of type Json in {ConfigFile.ConfigDict[ConfigFile.SOURCE_PATH_KEY]}");
            }
            _logger.LogInformation($"Will use '{FilesToLoad[0]}'");

            foreach (string filePath in FilesToLoad)
            {
                try
                {
                    //do try/catch because if there is more than one file and one of them is valid, I want to process it
                    string allFileText = File.ReadAllText(filePath);

                    Library newLibrary = JsonConvert.DeserializeObject<Library>(allFileText);

                    ValidateJsonFile.IsJsonFileValid(newLibrary);

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

    }
}
