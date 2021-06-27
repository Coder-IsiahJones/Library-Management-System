using Library.Data.Models;
using System.Collections.Generic;

namespace Library.Data
{
    public interface ILibraryCardService
    {
        IEnumerable<LibraryCard> GetAll();

        LibraryCard Get(int id);

        void Add(LibraryCard newCard);
    }
}