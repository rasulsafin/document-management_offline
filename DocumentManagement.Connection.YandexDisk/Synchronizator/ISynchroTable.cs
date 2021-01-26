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
        /// <param name="action">Информация об действии</param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        Task<List<ISynchroTable>> GetSubSynchroList(SyncAction action);

        /// <summary>
        /// Скачать с сервера объект
        /// </summary>
        /// <param name="action">Информация об действии </param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        Task Download(SyncAction action);

        /// <summary>
        /// Загрузить на сервер объект
        /// </summary>
        /// <param name="action">Информация об действии</param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        Task Upload(SyncAction action);

        /// <summary>
        /// Удалить объект из локальной коллекции
        /// </summary>
        /// <param name="action">Информация об действии</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task DeleteLocal(SyncAction action);

        /// <summary>
        /// Удалить объект из удаленной коллекции
        /// </summary>
        /// <param name="action">Информация об действии</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task DeleteRemote(SyncAction action);

        /// <summary>
        /// Получить информацию об особом виде синхронизации
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        SyncAction SpecialSynchronization(SyncAction action);

        /// <summary>
        /// Провести особый вид синхронизации
        /// </summary>
        /// <param name="action">Информация об действии</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Special(SyncAction action);
    }
}
