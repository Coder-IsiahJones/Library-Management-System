using Library.Data.Models;
using System.Collections.Generic;

namespace Library.Data
{
    public interface IHoldService
    {
        IEnumerable<AssetType> GetAll();

        AssetType Get(int id);

        void Add(AssetType newType);
    }
}