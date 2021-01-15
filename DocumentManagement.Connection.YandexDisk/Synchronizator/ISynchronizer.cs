using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.YandexDisk.Synchronizator
{
    public interface ISynchronizer
    {
        /// <summary>
        /// Вызывается один раз в начале работы, предназначент для получения коллекции объектов 
        /// или подключения к базе данных
        /// </summary>
        void LoadLocalCollect();

        /// <summary>
        /// Вызывается один раз в конце работы, предназначент для сохранения коллекции объектов 
        /// или отключения от базы данных
        /// </summary>
        void SaveLocalCollect();

        /// <summary>
        /// Возвращает существование записи id в удаленной коллекции, 
        /// <para>
        /// желательно сохранить полученую запись сразу после выполнения 
        /// будет вызвано <see cref="DownloadAndUpdateAsync(int)"/> 
        /// или <see cref="DeleteLocal(int)"/>         
        /// </para> 
        /// </summary>        
        Task<bool> RemoteExistAsync(int id);
        /// <summary>
        /// Удалить запись id из локальной коллекции
        /// </summary>
        /// <param name="id"></param>
        void DeleteLocal(int id);

        /// <summary>
        /// Загрузить запись с идентификатором id и добавить или обновить в локальную колекцию
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DownloadAndUpdateAsync(int id);
        /// <summary>
        /// Возвращает существование записи id в локальной коллекции, 
        /// <para>
        /// желательно сохранить полученую запись сразу после выполнения 
        /// будет вызвано <see cref="UpdateRemoteAsync(int)"/> 
        /// или <see cref="DeleteRemoteAsync(int)"/>         
        /// </para> 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool LocalExist(int id);
        /// <summary>
        /// Удалить запись id из удаленной коллекции
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteRemoteAsync(int id);
        /// <summary>
        /// загружает на сервер запист id 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task UpdateRemoteAsync(int id);  
        /// <summary>
        /// Возвращает синхронизатор внутренних коллекция
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        List<ISynchronizer> GetSubSynchronizes(int id);
        /// <summary>
        /// Получить список ревизий 
        /// </summary>
        /// <param name="revisions"></param>
        /// <returns></returns>
        List<Revision> GetRevisions(RevisionCollection revisions);

        /// <summary>
        /// Установить обновленную ревизию
        /// </summary>
        /// <param name="revisions"></param>
        /// <param name="local"></param>
        void SetRevision(RevisionCollection revisions, Revision rev);

    }
}