using NadavBookShopDataLoader.Models;
using System;
using System.Collections.Generic;


namespace NadavBookShopDataLoader.Interface
{
    interface IBooksRepository
    {
        
        public List<Book> GetNewBooks();

        //Set the rules to set a book valid to sell
        public bool IsBookValidToSell(Book b);

        //Set the rules to set if the data in a book is valid
        public string CheckBookDataIsValid(Book b);
      
        public string LoadDataToStore(List<Book> bookList);

        //Set who to round the price of the book the format to save it (for example $12)
        public string RoundPrice(decimal price);
    }
}
