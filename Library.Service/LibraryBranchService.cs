using Library.Data;
using Library.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Library.Service
{
    public class LibraryBranchService : ILibraryBranchService
    {
        private readonly LibraryDbContext _context;

        public LibraryBranchService(LibraryDbContext context)
        {
            _context = context;
        }

        public void Add(LibraryBranch newBranch)
        {
            _context.Add(newBranch);
            _context.SaveChanges();
        }

        public LibraryBranch Get(int branchId)
        {
            return GetAll()
                .FirstOrDefault(p => p.Id == branchId);
        }

        public IEnumerable<LibraryBranch> GetAll()
        {
            return _context.LibraryBranches
                .Include(a => a.Patrons)
                .Include(a => a.LibraryAssets);
        }

        public int GetAssetCount(int branchId)
        {
            return Get(branchId).LibraryAssets.Count();
        }

        public IEnumerable<LibraryAsset> GetAssets(int branchId)
        {
            return _context.LibraryBranches
                .Include(a => a.LibraryAssets)
                .FirstOrDefault(b => b.Id == branchId)
                .LibraryAssets;
        }

        public decimal GetAssetsValue(int branchId)
        {
            var assetsValue = GetAssets(branchId)
                .Select(a => a.Cost);
            return assetsValue.Sum();
        }

        public IEnumerable<string> GetBranchHours(int branchId)
        {
            var hours = _context.BranchHours
                .Where(h => h.Branch.Id == branchId);

            return DataHelpers.HumanizeBusinessHours(hours);
        }

        public int GetPatronCount(int branchId)
        {
            return Get(branchId).Patrons.Count();
        }

        public IEnumerable<Patron> GetPatrons(int branchId)
        {
            return _context.LibraryBranches
                .Include(a => a.Patrons)
                .FirstOrDefault(b => b.Id == branchId)
                .Patrons;
        }

        //TODO: Implement
        public bool IsBranchOpen(int branchId)
        {
            var currentTimeHour = DateTime.Now.Hour;
            var currentDayOfWeek = (int)DateTime.Now.DayOfWeek + 1;

            var hours = _context.BranchHours
                .Where(h => h.Branch.Id == branchId);

            var daysHour = hours.FirstOrDefault(h => h.DayOfWeek == currentDayOfWeek);

            return currentTimeHour < daysHour.CloseTime && currentTimeHour > daysHour.OpenTime;
        }
    }
}