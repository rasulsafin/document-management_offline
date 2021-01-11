using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.YandexDisk.Synchronizer
{
    public class SyncManager
    {

    }

    public class Transaction
    {       
        public ulong Rev { get ; set ;  }
        public TransType Type { get; set; }
        public Table Table { get; set; }
        public int IdObject { get; set; }
        //public int IdProject { get; set; }

        [JsonIgnore]
        public bool Sync { get; set; }
        /// <summary>
        /// Изменение на сервере
        /// </summary>
        [JsonIgnore]
        public bool Server { get; set; }
    }

    public enum TransType
    {
        Update,
        Delete
    }
    public enum Table
    {
        Project,
        Objective,
        Item
    }
}
