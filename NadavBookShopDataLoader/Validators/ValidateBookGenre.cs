using NadavBookShopDataLoader.Interfaces;
using NadavBookShopDataLoader.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NadavBookShopDataLoader.Validators
{
    public class ValidateBookGenre : IValidate
    {
        public string ValidationsName { get; set; }

        public ValidateBookGenre()
        {
            ValidationsName = this.GetType().Name;
        }
        //array of valid Genres 
        string[] validGenres = new string[] { "computer", "fantasy", "horror" };  //set all string is lowerCase
        public bool IsBookValid(Book book)
        {
            if (Array.IndexOf(validGenres, book.genre.Trim().ToLower()) == -1)
            {
                throw new BookValidationException($"Genre = {book.genre} is no valid", ValidationsName);
            }

            return true;
        }
    }
}
