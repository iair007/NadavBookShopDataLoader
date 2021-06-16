using NadavBookShopDataLoader.Interfaces;
using NadavBookShopDataLoader.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NadavBookShopDataLoader.Validators
{
    public static class ValidateJsonFile 
    {
        //array of valid Genres 
        /// <summary>
        /// will check that all the properties of the Book have data.
        /// That the publish_date is a valid Date
        /// That the price is a valid decimal
        /// </summary>
        /// <param name="book">The Book to check if valid or not</param>
        /// <returns></returns>
        public static bool IsJsonFileValid(Library newLibrary)
        {
            if (newLibrary == null)
                throw new FormatException($"json file is incorrect");

            if (newLibrary.catalog == null)
                throw new FormatException($"json file is missing Catalog");

            if (newLibrary.catalog.book == null)
                throw new FormatException($"json file is missing Book[]");

            return true;  //if got here the files is OK
        }
    }
}
