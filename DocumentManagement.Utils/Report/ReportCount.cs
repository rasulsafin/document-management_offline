using static MRS.DocumentManagement.Utils.PreferenceHandler;

namespace MRS.DocumentManagement.Utils.Reports
{
    public class ReportCount
    {
        public string ID
        {
            get => Get(CreateKeyByMethodName(nameof(ReportCount)), ref id);
            set => Set(CreateKeyByMethodName(nameof(ReportCount)), out id, value);
        }
        
        public int? Count
        {
            get => Get(CreateKeyByMethodName(nameof(ReportCount)), ref count);
            set => Set(CreateKeyByMethodName(nameof(ReportCount)), out count, value);
        }

        private string id;
        private int? count;
    }
}