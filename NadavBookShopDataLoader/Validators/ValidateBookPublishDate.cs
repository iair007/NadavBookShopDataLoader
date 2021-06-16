using NadavBookShopDataLoader.Interfaces;
using NadavBookShopDataLoader.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NadavBookShopDataLoader.Validators
{

    public class ValidateBookPublishDate : IValidate
    {
        public string ValidationsName { get; set; }

        public ValidateBookPublishDate()
        {
            ValidationsName = this.GetType().Name;
        }
        /// <summary>
        /// A valid book is a book that was not published on Saturday
        /// and the Author's name does not include the word 'Peter' (or any other word in _skipAuthors)
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        public bool IsBookValid(Book book)
        {
            DateTime publishDate;

            if (DateTime.TryParse(book.publish_date, out publishDate) == false)
            {
                throw new BookValidationException($"Value= {book.publish_date} is not a valid Date" ,"publish_date");
            }

            bool isValid = publishDate.DayOfWeek != DayOfWeek.Saturday;

            return isValid;
        }
    }
}
