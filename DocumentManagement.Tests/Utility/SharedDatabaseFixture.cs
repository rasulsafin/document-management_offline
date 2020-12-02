using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using MRS.DocumentManagement.Database;

namespace MRS.DocumentManagement.Tests.Utility
{
    public class SharedDatabaseFixture : IDisposable
    {
        private static readonly object _lock = new object();
        private static bool _databaseInitialized;

        public DbConnection Connection { get; }

        public SharedDatabaseFixture(Action<DMContext> seedAction = null)
        {
            Connection = new SqliteConnection("DataSource = file::memory:?cache = shared");
            //Connection = new Npgsql.NpgsqlConnection("Server=127.0.0.1;Port=5432;Database=DocumentManagement;User Id=postgres;Password=123;");
            Connection.Open();
            Seed(seedAction);
        }

        public DMContext CreateContext(DbTransaction transaction = null)
        {
            var context = new DMContext(new DbContextOptionsBuilder<DMContext>().UseSqlite(Connection).Options);
            //var context = new DMContext(new DbContextOptionsBuilder<DMContext>().UseNpgsql(Connection).Options);

            if (transaction != null)
            {
                context.Database.UseTransaction(transaction);
            }

            return context;
        }

        private void Seed(Action<DMContext> seed)
        {
            lock (_lock)
            {
                // FIXME: this check is needed when using shared connection to single database
                // but SQLite in-memory DB is recreated every time when new connection is set
                // so this check would prevent database to be initialized correctly
                //if (!_databaseInitialized)
                {
                    using (var context = CreateContext())
                    {
                        context.Database.EnsureDeleted();
                        context.Database.EnsureCreated();
                        //context.Database.Migrate();

                        // add initial database data here
                        seed?.Invoke(context);
                    }

                    _databaseInitialized = true;
                }
            }
        }

        public void Dispose() => Connection.Dispose();
    }
}
