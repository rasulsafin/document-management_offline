using System.Threading.Tasks;

namespace DocumentManagement.Connection.Tests
{
    public interface IUserSynchroTests
    {
        void CheckDBRevisionTest();
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