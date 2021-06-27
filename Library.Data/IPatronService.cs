using Library.Data.Models;
using System.Collections.Generic;

namespace Library.Data
{
    public interface IPatronService
    {
        IEnumerable<Patron> GetAll();

        Patron Get(int id);

        void Add(Patron newBook);

        IEnumerable<CheckoutHistory> GetCheckoutHistory(int patronId);

        IEnumerable<Hold> GetHolds(int patronId);

        IEnumerable<Checkout> GetCheckouts(int id);
    }
}