using System.Threading.Tasks;

namespace DocumentManagement.Connection.Tests
{
    public interface IUserSynchroTests
    {
        Task DeleteLocalTest();
        Task DeleteRemoteTest();
        Task DownloadTestExist();
        Task DownloadTestNotExist();
        void GetRevisionsTest();
        Task GetSubSynchroListTest();
        void SetRevisionTest();
        void SpecialSynchronizationTest();
        Task SpecialTest();
        Task UploadTest();
    }
}