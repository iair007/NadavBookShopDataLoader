using NadavBookShopDataLoader.Models;


namespace NadavBookShopDataLoader.Interfaces
{
    //Used to create new validations class
    interface IValidate
    {
        string ValidationsName { get; set; }
        bool IsBookValid(Book book);
    }
}
