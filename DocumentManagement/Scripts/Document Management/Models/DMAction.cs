using LiteDB;
using System;

namespace MRS.Bim.DocumentManagement
{
    [Serializable]
    public class DMAction
    {
        [BsonId]
        public int ID { get; set; }
        public dynamic ProjectId { get; set; }
        public string MethodName { get; set; }
        public object[] Parameters { get; set; }
    }
}