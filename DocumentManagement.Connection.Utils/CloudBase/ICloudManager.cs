using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.Utils
{
    public interface ICloudManager
    {
        /// <summary>
        /// Скачать объект с сервера из соответвующей таблицы. Если объекта нет вернётся null;
        /// </summary>
        /// <typeparam name="T"> Тип объекта и имя папки </typeparam>
        /// <param name="id"> id объекта </param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<T> Pull<T>(string id);

        /// <summary>
        /// Отправить объект на сервер в папку соответвующую названию Типа
        /// </summary>
        /// <typeparam name="T">Тип объекта и имя папки</typeparam>
        /// <param name="object"> конеченый объект </param>
        /// <param name="id">id объекта</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<bool> Push<T>(T @object, string id);

        /// <summary>
        /// Удалить файл
        /// </summary>
        /// <param name="href">уникальный путь к файлу</param>
        /// <returns></returns>
        Task<bool> DeleteFile(string href);

        /// <summary>
        /// Получить файл
        /// </summary>
        /// <param name="href">уникальный путь к файлу</param>
        /// <param name="fileName">имя файла</param>
        /// <returns></returns>
        Task<bool> PullFile(string href, string fileName);

        /// <summary>
        /// Отправить файл
        /// </summary>
        /// <param name="remoteDirName">Название подпапки в папке приложения 'BRIO MRS' </param>
        /// <param name="localDirName">Локальный путь (папка должна быть создана)</param>
        /// <param name="fileName">имя файла</param>
        /// <returns></returns>
        Task<string> PushFile(string remoteDirName, string localDirName, string fileName);

        /// <summary>
        /// Удалить объект из папки
        /// </summary>
        /// <typeparam name="T">Тип объекта и имя папки</typeparam>
        /// <param name="id">id объекта</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<bool> Delete<T>(string id);
    }
}
