using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection
{
    public interface IDiskManager
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
        /// Удалить объект из папки
        /// </summary>
        /// <typeparam name="T">Тип объекта и имя папки</typeparam>
        /// <param name="id">id объекта</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete<T>(string id);
    }
}