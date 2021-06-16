using NadavBookShopDataLoader.Models;
using System.Collections.Generic;


namespace NadavBookShopDataLoader.Interface
{
    interface IGetBooksRepository
    {
        public List<Book> GetNewBooksFromSource();

    }
}
