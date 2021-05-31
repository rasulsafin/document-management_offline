using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Converters
{
    public delegate Task<TOutput> ConverterAsync<in TInput, TOutput>(TInput input);

    /// <summary>
    /// Represents a class that converts an object from one type to another type.
    /// </summary>
    /// <typeparam name="TFrom">The type of object that is to be converted.</typeparam>
    /// <typeparam name="TTo">The type the input object is to be converted to.</typeparam>
    internal interface IConverter<in TFrom, TTo>
    {
        /// <summary>
        /// Converts an object from one type to another type.
        /// </summary>
        /// <param name="from">The object to convert.</param>
        /// <returns>The task of the operation.</returns>
        public Task<TTo> Convert(TFrom from);
    }
}
