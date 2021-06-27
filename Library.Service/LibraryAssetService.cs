using Library.Data;
using Library.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Library.Service
{
    public class LibraryAssetService : ILibraryAssetService
    {
        private readonly LibraryDbContext _context;

        public LibraryAssetService(LibraryDbContext context)
        {
            _context = context;
        }

        public void Add(LibraryAsset newAsset)
        {
            _context.Add(newAsset);
            _context.SaveChanges();
        }

        public LibraryAsset GetById(int id)
        {
            return GetAll()
                .FirstOrDefault(asset => asset.Id == id);
        }

        public IEnumerable<LibraryAsset> GetAll()
        {
            return _context.LibraryAssets
                .Include(asset => asset.Status)
                .Include(asset => asset.Location);
        }

        public LibraryBranch GetCurrentLocation(int id)
        {
            return GetById(id).Location;
        }

        public string GetDeweyIndex(int id)
        {
            if (_context.Books.Any(book => book.Id == id))
            {
                return _context.Books.FirstOrDefault(book => book.Id == id).DeweyIndex;
            }
            else return "";
        }

        public string GetIsbn(int id)
        {
            if (_context.Books.Any(book => book.Id == id))
            {
                return _context.Books.FirstOrDefault(book => book.Id == id).ISBN;
            }
            else return "";
        }

        public string GetTitle(int id)
        {
            return _context.LibraryAssets.First(a => a.Id == id).Title;
        }

        public LibraryCard GetLibraryCardByAssetId(int id)
        {
            return _context.LibraryCards
                .FirstOrDefault(c => c.Checkouts
                    .Select(a => a.LibraryAsset)
                    .Select(v => v.Id).Contains(id));
        }

        public string GetType(int id)
        {
            // Hack
            var book = _context.LibraryAssets
                .OfType<Book>().Where(b => b.Id == id);

            return book.Any() ? "Book" : "Video";
        }

        private string GetAuthor(int id)
        {
            var book = (Book)GetById(id);
            return book.Author;
        }

        private string GetDirector(int id)
        {
            var video = (Video)GetById(id);
            return video.Director;
        }

        public string GetAuthorOrDirector(int id)
        {
            var isBook = _context.LibraryAssets
                .OfType<Book>().Where(asset => asset.Id == id).Any();

            var isVideo = _context.LibraryAssets
                 .OfType<Video>().Where(asset => asset.Id == id).Any();

            return isBook ?
                _context.Books.FirstOrDefault(book => book.Id == id).Author :
                _context.Videos.FirstOrDefault(Video => Video.Id == id).Director
                ?? "Unknown";
        }
    }
}