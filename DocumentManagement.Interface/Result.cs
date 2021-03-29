namespace MRS.DocumentManagement.Interface
{
    public class Result<T> : IResult
    {
        public Result(T value) => Value = value;

        public T Value { get; private set; }
    }
}
