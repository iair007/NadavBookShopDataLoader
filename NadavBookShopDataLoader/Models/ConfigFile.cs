using System;
using System.Collections.Generic;
using System.Text;

namespace NadavBookShopDataLoader.Models
{
   public static class ConfigFile
    {
        public static Dictionary<string, string> ConfigDict = new Dictionary<string, string>();
        public static string SOURCE_PATH_KEY = "SourcePath";
        public static string TARGET_PATH_KEY = "TargetPath";
        public static string SOURCE_FILE_NAME_KEY = "SourceFileName";
        public static string SOURCE_FILE_NAME = "books.json";
    }
}
