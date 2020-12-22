using AutoMapper;
using MRS.DocumentManagement;

namespace MRS.DocumentManagement.Utility
{
    public class IDTypeConverter<T> : ITypeConverter<ID<T>, int>
    {
        public int Convert(ID<T> source, int destination, ResolutionContext context)
            => (int)source;
    }

    public class IntIDTypeConverter<T> : ITypeConverter<int, ID<T>>
    {
        public ID<T> Convert(int source, ID<T> destination, ResolutionContext context)
            => (ID<T>)source;
    }
}