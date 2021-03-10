using System;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;

namespace MRS.DocumentManagement.Tests.Utility
{
    public class SharedDatabaseFixture : IDisposable
    {
        private static readonly object _lock = new object();
        private static bool _databaseInitialized;

        public DMContext Context { get; }
        private DbConnection Connection { get; set; }

        private DbContextOptions<DMContext> options;

        public SharedDatabaseFixture(Action<DMContext> seedAction = null)
        {
            Context = Seed(seedAction);
            Connection.Open();
            //Connection = RelationalOptionsExtension.Extract(options).Connection;
        }

        public DMContext CreateContext(DbTransaction transaction = null)
        {
            options = new DbContextOptionsBuilder<DMContext>().UseSqlite(CreateInMemoryDatabase()).Options;
            var context = new DMContext(options);

            if (transaction != null)
                context.Database.UseTransaction(transaction);

            return context;
        }

        public void Dispose()
        {
            if (Connection != null)
                Connection.Dispose();

            if (Context != null)
                Context.Dispose();
        }

        private DMContext Seed(Action<DMContext> seed)
        {
            lock (_lock)
            {
                //// FIXME: this check is needed when using shared connection to single database
                //// but SQLite in-memory DB is recreated every time when new connection is set
                //// so this check would prevent database to be initialized correctly
                //if (!_databaseInitialized)
                //{
                    DMContext context = CreateContext();
                    //context.Database.EnsureDeleted();
                    //context.Database.EnsureCreated();
                    //context.Database.Migrate();

                    // add initial database data here
                    seed?.Invoke(context);
                    //_databaseInitialized = true;

                    return context;
                //}
            }
            //return null;
        }

        private DbConnection CreateInMemoryDatabase()
        {
            Connection = new SqliteConnection("Data Source = DocumentManagement.db;Filename=:memory:");
            //Connection.Open();

            return Connection;
        }
    }
}
