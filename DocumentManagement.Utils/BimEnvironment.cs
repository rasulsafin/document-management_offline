using System;
using MRS.Bim.Tools;

namespace MRS.DocumentManagement.Utils
{
    public class BimEnvironment
    {
        public static BimEnvironment Instance => INSTANCE_CONTAINER.Value;
        private static readonly Lazy<BimEnvironment> INSTANCE_CONTAINER = new Lazy<BimEnvironment>(() => new BimEnvironment());

        public string StreamingAssetsPath { internal get; set; }
        
        public Action<string, object> SaveSettingsAction { internal get; set; }
        public Func<string, Type, object> GetSettingsFunc { internal get; set; }
        public Func<string, Type, object> LoadResource { internal get; set; }
        public Func<IProgressing> GetProgressor { internal get; set; }
                
        private BimEnvironment()
        { }
    }
}