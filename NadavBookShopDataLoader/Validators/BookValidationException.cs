using System;
using System.Collections.Generic;
using System.Text;

namespace NadavBookShopDataLoader.Validators
{
    public class BookValidationException : Exception
    {
        public string InvalidProperty { get; set; }

        public BookValidationException(string Message, string propetyName) : base(Message)
        {
            InvalidProperty = propetyName;
        }
    }
}
