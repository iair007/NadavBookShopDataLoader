using NadavBookShopDataLoader.Models;
using System.Collections.Generic;


namespace NadavBookShopDataLoader.Interface
{
    interface ISaveBooksRepository
    {
        public string SaveBooks(List<Book> bookList, string targetDir, string fileName);

    }
}
