using NadavBookShopDataLoader.Interfaces;
using NadavBookShopDataLoader.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NadavBookShopDataLoader.Validators
{
    public class ValidateBookPrice : IValidate
    {
        public string ValidationsName { get; set; }

        public ValidateBookPrice()
        {
            ValidationsName = this.GetType().Name;
        }
        public bool IsBookValid(Book book)
        {
            decimal price;

            if (decimal.TryParse(book.price, out price) == false)
            {
                throw new BookValidationException($"Value= {book.price} is not a valid Decimal","publish_date");
            }
            else
            {
                book.price = RoundPrice(price);
            }

            return true;
        }

        private string RoundPrice(decimal price)
        {
            return Math.Ceiling(price).ToString("$#");
        }
    }
}
