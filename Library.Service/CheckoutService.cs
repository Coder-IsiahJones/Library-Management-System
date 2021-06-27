using Library.Data;
using Library.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Library.Service
{
    public class CheckoutService : ICheckoutService
    {
        private readonly LibraryDbContext _context;

        public CheckoutService(LibraryDbContext context)
        {
            _context = context;
        }

        public void Add(Checkout newCheckout)
        {
            _context.Add(newCheckout);
            _context.SaveChanges();
        }

        public IEnumerable<Checkout> GetAll()
        {
            return _context.Checkouts;
        }

        public IEnumerable<CheckoutHistory> GetCheckoutHistory(int id)
        {
            return _context.CheckoutHistories
                .Include(a => a.LibraryAsset)
                .Include(a => a.LibraryCard)
                .Where(a => a.LibraryAsset.Id == id);
        }

        public IEnumerable<Hold> GetCurrentHolds(int id)
        {
            return _context.Holds
                .Include(h => h.LibraryAsset)
                .Where(a => a.LibraryAsset.Id == id);
        }

        public Checkout GetById(int id)
        {
            return GetAll()
                .FirstOrDefault(p => p.Id == id);
        }

        public Checkout GetLatestCheckout(int id)
        {
            return _context.Checkouts
                 .Where(c => c.LibraryAsset.Id == id)
                 .OrderByDescending(c => c.Since)
                 .FirstOrDefault();
        }

        public string GetCurrentCheckoutPatron(int id)
        {
            var checkout = GetCheckoutByAssetId(id);

            if (checkout == null)
            {
                return "";
            }

            var cardId = checkout.LibraryCard.Id;

            var patron = _context.Patrons
                .Include(p => p.LibraryCard)
                .FirstOrDefault(p => p.LibraryCard.Id == cardId);

            return patron.FirstName + " " + patron.LastName;
        }

        public string GetCurrentHoldPatronName(int id)
        {
            var hold = _context.Holds
                .Include(a => a.LibraryAsset)
                .Include(a => a.LibraryCard)
                .FirstOrDefault(v => v.Id == id);

            var cardId = hold?.LibraryCard.Id;

            var patron = _context.Patrons
                .Include(p => p.LibraryCard)
                .FirstOrDefault(p => p.LibraryCard.Id == cardId);

            return patron?.FirstName + " " + patron?.LastName;
        }

        public DateTime GetCurrentHoldPlaced(int id)
        {
            return
                _context.Holds
                .Include(a => a.LibraryAsset)
                .Include(a => a.LibraryCard)
                .FirstOrDefault(v => v.Id == id)
                .HoldPlaced;
        }

        public void CheckOutItem(int assetId, int libraryCardId)
        {
            if (IsCheckedOut(assetId))
            {
                return;
                // Add logic here to handle feedback to the user.
            }

            var item = _context.LibraryAssets
                .FirstOrDefault(a => a.Id == assetId);

            UpdateAssetStatus(assetId, "Checked Out");

            var libraryCard = _context.LibraryCards
               .Include(c => c.Checkouts)
               .FirstOrDefault(a => a.Id == libraryCardId);

            item.Status = _context.Statuses
                .FirstOrDefault(a => a.Name == "Checked Out");

            var now = DateTime.Now;

            var checkout = new Checkout
            {
                LibraryAsset = item,
                LibraryCard = libraryCard,
                Since = now,
                Until = GetDefaultCheckoutTime(now)
            };

            _context.Add(checkout);

            var checkoutHistory = new CheckoutHistory
            {
                CheckedOut = now,
                LibraryAsset = item,
                LibraryCard = libraryCard
            };

            _context.Add(checkoutHistory);
            _context.SaveChanges();
        }

        public void CheckInItem(int assetId)
        {
            var now = DateTime.Now;

            var item = _context.LibraryAssets
                .FirstOrDefault(a => a.Id == assetId);

            // remove any existing checkouts on the item
            RemoveExistingCheckouts(assetId);

            // close any existing checkout history
            CloseExistingCheckoutHistory(assetId, now);

            // look for current holds
            var currentHolds = _context.Holds
                .Include(a => a.LibraryAsset)
                .Include(a => a.LibraryCard)
                .Where(a => a.LibraryAsset.Id == assetId);

            // if there are current holds, check out the item to the earliest
            if (currentHolds.Any())
            {
                CheckoutToEarliestHold(assetId, currentHolds);
                return;
            }

            // otherwise, set item status to available
            UpdateAssetStatus(assetId, "Available");

            _context.SaveChanges();
        }

        public void PlaceHold(int assetId, int libraryCardId)
        {
            var now = DateTime.Now;

            var asset = _context.LibraryAssets
                .Include(a => a.Status)
                .FirstOrDefault(a => a.Id == assetId);

            var card = _context.LibraryCards
                .FirstOrDefault(a => a.Id == libraryCardId);

            if (asset.Status.Name == "Available")
            {
                UpdateAssetStatus(assetId, "On Hold");
            }

            var hold = new Hold
            {
                HoldPlaced = now,
                LibraryAsset = asset,
                LibraryCard = card
            };

            _context.Add(hold);
            _context.SaveChanges();
        }

        public void MarkLost(int assetId)
        {
            UpdateAssetStatus(assetId, "Lost");

            _context.SaveChanges();
        }

        public void MarkFound(int assetId)
        {
            var now = DateTime.Now;

            UpdateAssetStatus(assetId, "Available");

            RemoveExistingCheckouts(assetId);

            CloseExistingCheckoutHistory(assetId, now);

            _context.SaveChanges();
        }

        // Broken up methods
        private DateTime GetDefaultCheckoutTime(DateTime now)
        {
            return now.AddDays(30);
        }

        private void UpdateAssetStatus(int assetId, string newStatus)
        {
            var item = _context.LibraryAssets
                .FirstOrDefault(a => a.Id == assetId);

            _context.Update(item);
            item.Status = _context.Statuses.FirstOrDefault(a => a.Name == newStatus);
        }

        private void RemoveExistingCheckouts(int assetId)
        {
            // remove any existing checkouts on the item
            var checkout = _context.Checkouts
                .FirstOrDefault(a => a.LibraryAsset.Id == assetId);
            if (checkout != null) _context.Remove(checkout);
        }

        private void CloseExistingCheckoutHistory(int assetId, DateTime now)
        {
            // close any existing checkout history
            var history = _context.CheckoutHistories
                .FirstOrDefault(h =>
                    h.LibraryAsset.Id == assetId
                    && h.CheckedIn == null);
            if (history != null)
            {
                _context.Update(history);
                history.CheckedIn = now;
            }
        }

        public int GetNumberOfCopies(int id)
        {
            return _context.LibraryAssets
                .FirstOrDefault(a => a.Id == id)
                .NumberOfCopies;
        }

        public bool IsCheckedOut(int id)
        {
            return _context.Checkouts
                .Where(a => a.LibraryAsset.Id == id)
                .Any();
        }

        private void CheckoutToEarliestHold(int assetId, IQueryable<Hold> currentHolds)
        {
            var earliestHold = currentHolds
                .OrderBy(holds => holds.HoldPlaced)
                .FirstOrDefault();

            var card = earliestHold.LibraryCard;

            _context.Remove(earliestHold);
            _context.SaveChanges();

            CheckOutItem(assetId, card.Id);
        }

        private Checkout GetCheckoutByAssetId(int assesId)
        {
            return _context.Checkouts
                .Include(co => co.LibraryAsset)
                .Include(co => co.LibraryCard)
                .FirstOrDefault(co => co.LibraryAsset.Id == assesId);
        }
    }
}