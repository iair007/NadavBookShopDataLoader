using NadavBookShopDataLoader.Interfaces;
using NadavBookShopDataLoader.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NadavBookShopDataLoader.Validators
{
    public class ValidateBookData : IValidate
    {
        public string ValidationsName { get; set; }

        public ValidateBookData()
        {
            ValidationsName = this.GetType().Name;
        }
        //array of valid Genres 
        string[] validGenres = new string[] { "Computer", "Fantasy", "Fantasy", "Horror" };
        public bool IsBookValid(Book book)
        {
            if (book.Id == string.Empty)
            {
                throw new Exception($"Book from author='{book.author}' with title= '{book.title}' is missing and 'id'");
            }
            if (book.publish_date == string.Empty)
            {
                throw new BookValidationException("Property is empty","publish_date");
            }
            if (book.price == string.Empty)
            {
                throw new BookValidationException("Property is empty", "price");
            }

            if (book.title == string.Empty)
            {
                throw new BookValidationException("Property is empty", "title");
            }
            if (book.genre == string.Empty)
            {
                throw new BookValidationException("Property is empty", "genre");
            }
            if (book.description == string.Empty)
            {
                throw new BookValidationException("Property is empty", "description");
            }
            if (book.author == string.Empty)
            {
                throw new BookValidationException("Property is empty", "author");
            }

            return true;
        }
    }
}
