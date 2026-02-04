namespace MyApi.CutomException
{
    public class ApiServiceException : Exception
    {
        public int? StatusCode { get; }
        public string ApiName { get; }

        public ApiServiceException(string message, int? statusCode = null)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
