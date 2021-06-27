using Library.Data.Models;
using System.Collections.Generic;

namespace Library.Data
{
    public interface IBookService
    {
        IEnumerable<Book> GetAll();

        IEnumerable<Book> GetByAuthor(string author);

        IEnumerable<Book> GetByIsbn(string isbn);

        Book Get(int id);

        void Add(Book newBook);
    }
}