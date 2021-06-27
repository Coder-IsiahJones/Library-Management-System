using Library.Data.Models;
using System;
using System.Collections.Generic;

namespace Library.Data
{
    public interface ICheckoutService
    {
        void Add(Checkout newCheckout);

        IEnumerable<Checkout> GetAll();

        IEnumerable<CheckoutHistory> GetCheckoutHistory(int id);

        IEnumerable<Hold> GetCurrentHolds(int id);

        Checkout GetById(int checkoutId);

        Checkout GetLatestCheckout(int id);

        void CheckOutItem(int id, int libraryCardId);

        void CheckInItem(int id);

        void PlaceHold(int assetId, int libraryCardId);

        void MarkLost(int id);

        void MarkFound(int id);

        string GetCurrentCheckoutPatron(int id);

        string GetCurrentHoldPatronName(int id);

        DateTime GetCurrentHoldPlaced(int id);

        int GetNumberOfCopies(int id);

        bool IsCheckedOut(int id);
    }
}