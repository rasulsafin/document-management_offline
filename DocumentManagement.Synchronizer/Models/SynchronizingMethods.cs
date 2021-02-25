using Microsoft.EntityFrameworkCore.ChangeTracking;
using MRS.DocumentManagement.Database.Models;

namespace MRS.DocumentManagement.Synchronizer.Models
{
    public class SynchronizingMethods
    {
        public delegate EntityEntry<T> DBFunc<T>()
                where T : class;

        public DBFunc<Objective> Type { get; set; }
    }
}
