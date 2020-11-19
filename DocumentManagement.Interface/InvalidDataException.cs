namespace DocumentManagement.Interface
{
    [System.Serializable]
    public class InvalidDataException : System.Exception
    {
        public InvalidDataException() { }
        public InvalidDataException(string message) : base(message) { }
        public InvalidDataException(string message, System.Exception inner) : base(message, inner) { }
        protected InvalidDataException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
