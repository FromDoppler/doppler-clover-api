using System;

namespace Doppler.CloverAPI.Exceptions
{
    public class CloverApiException : Exception
    {
        public string Code { get; set; }
        public string Message { get; set; }

        public ApiError ApiError { get; set; }

        public CloverApiException(string code, string message) : base(message)
        {
            Code = code;
            Message = message;
        }
    }

    public class ApiError
    {
        public string Message { get; set; }
        public ApiErrorCause Error { get; set; }

    }

    public class ApiErrorCause
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }
}
