using NadavBookShopDataLoader.Interfaces;
using NadavBookShopDataLoader.Models;

namespace NadavBookShopDataLoader.Validators
{
    public class ValidateBooksAuthor : IValidate
    {
        public string ValidationsName { get; set; }

        public ValidateBooksAuthor()
        {
            ValidationsName = this.GetType().Name;
        }

        //Authors that dont allow to save in store
        string[] SKIP_AUTHORS = new string[] { "peter" };  //write always in lowerCase


        public bool IsBookValid(Book book)
        {
            bool isValid = true;
            foreach (string skipAutor in SKIP_AUTHORS)
            {
                if (book.author.ToLower().Contains(skipAutor))
                {
                    throw new BookValidationException($"The Store does not allow to seel book from author {book.author}",ValidationsName);
                }
            }
            return isValid;
        }
    }
}
