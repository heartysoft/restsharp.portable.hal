namespace Hal.ErrorHandling
{
    public class ValidationError
    {
        public string Key { get; private set; }
        public string Message { get; private set; }

        public ValidationError(string key, string message)
        {
            Key = key;
            Message = message;
        }
    }
}