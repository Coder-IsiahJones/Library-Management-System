using Library.Data.Models;
using System.Collections.Generic;

namespace Library.Data
{
    public interface IStatusService
    {
        IEnumerable<Status> GetAll();

        Status Get(int id);

        void Add(Status newStatus);
    }
}