using MRS.DocumentManagement.Interface.Exceptions;

namespace MRS.DocumentManagement.Exceptions
{
    public class NotFoundException<T> : ANotFoundException
    {
        public NotFoundException(int id)
            : base($"{typeof(T).Name} with key {id} not found")
        {
        }

        public NotFoundException(string propertyName, string propertyValue)
            : base($"{typeof(T).Name} with {propertyName.ToLower()} {propertyValue} not found")
        {
        }
    }
}
