using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    /// <summary>
    /// Синхронизатор
    /// </summary>
    public interface ISynchroTable
    {

        /// <summary>
        /// Получить список ревизий.
        /// </summary>
        /// <param name="revisions">Комплексная переменная из которой нужно извлечь коллекцию.</param>
        /// <returns> Получить список ревизий </returns>
        List<Revision> GetRevisions(RevisionCollection revisions);


        /// <summary>
        /// Установить обновленную ревизию.
        /// </summary>
        /// <param name="revisions">Комплексная переменная в которую нужно записать коллекцию.</param>
        /// <param name="rev"> экземпляр ревизии </param>
        void SetRevision(RevisionCollection revisions, Revision rev);

        /// <summary>
        /// Возвращает синхронизатор внутренних коллекция.
        /// </summary>
        /// <param name="id">id записи.</param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        Task<List<ISynchroTable>> GetSubSynchroList(SyncAction action);
        Task Download(SyncAction action);
        Task Upload(SyncAction action);
        Task DeleteLocal(SyncAction action);
        Task DeleteRemote(SyncAction action);
        SyncAction SpecialSynchronization(SyncAction action);
        Task Special(SyncAction action);
    }
}
