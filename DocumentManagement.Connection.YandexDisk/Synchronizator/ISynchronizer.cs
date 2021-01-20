using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    /// <summary>
    /// Синхронизатор
    /// </summary>
    public interface ISynchronizer
    {
        /// <summary>
        /// Вызывается один раз в начале работы, предназначент для получения коллекции объектов
        /// или подключения к базе данных.
        /// </summary>
        void LoadCollection();

        /// <summary>
        /// Вызывается один раз в конце работы, предназначент для сохранения коллекции объектов
        /// или отключения от базы данных.
        /// </summary>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        Task SaveLocalCollectAsync();

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
        /// Выбрать действие которое следует сделать
        /// </summary>
        /// <param name="localRev"> null или экземпляр локальной ревизии </param>
        /// <param name="remoteRev"> null или экземпляр удаленной ревизии </param>
        /// <returns> действие которое следует сделать </returns>
        Task<SyncAction> GetActoin(Revision localRev, Revision remoteRev);

        /// <summary>
        /// Возвращает синхронизатор внутренних коллекция.
        /// </summary>
        /// <param name="id">id записи.</param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        Task<List<ISynchronizer>> GetSubSynchronizesAsync(int id);

        /// <summary>
        /// Скачать запись с идентификатором id и добавить или обновить в локальной колекции.
        /// </summary>
        /// <param name="id">id записи.</param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        Task DownloadRemote(int id);

        /// <summary>
        /// загружает на сервер запись по id.
        /// </summary>
        /// <param name="id">id записи.</param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        Task UploadLocal(int id);

        /// <summary>
        /// Удалить запись из локальной коллекции.
        /// </summary>
        /// <param name="id">id записи.</param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        Task DeleteLocal(int id);

        /// <summary>
        /// Удалить запись id из удаленной коллекции.
        /// </summary>
        /// <param name="id">id записи.</param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        Task DeleteRemote(int id);

        ///// <summary>
        ///// Возвращает существование записи id в удаленной коллекции,.
        ///// <para>
        ///// желательно сохранить полученую запись сразу после выполнения
        ///// будет вызвано <see cref="DownloadRemote(int)"/>
        ///// или <see cref="DeleteLocal(int)"/>.
        ///// </para>
        ///// </summary>
        ///// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        // Task<bool> RemoteExist(int id);
        ///// <summary>
        ///// Возвращает существование записи id в локальной коллекции,.
        ///// <para>
        ///// желательно сохранить полученую запись сразу после выполнения
        ///// будет вызвано <see cref="UploadLocal(int)"/>
        ///// или <see cref="DeleteRemote(int)"/>.
        ///// </para>
        ///// </summary>
        ///// <param name="id">id записи.</param>
        ///// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        // Task<bool> LocalExist(int id);
    }
}
