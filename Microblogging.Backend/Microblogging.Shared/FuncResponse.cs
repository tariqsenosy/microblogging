using Microblogging.Shared;
using System.Net;


namespace Microblogging.Shared
{
    public class FuncResponse(HttpStatusCode statusCode = HttpStatusCode.OK, ResponseCode responseCode = ResponseCode.Success, string message = "")
    {
        public string Message { get; set; } = message;
        private ResponseCode ResponseCode { get; set; } = responseCode;
        public HttpStatusCode HttpStatusCode { get; set; } = statusCode;
        public bool IsSuccess => ResponseCode == ResponseCode.Success;
    }
}

public class FuncResponseWithValue<T>(T data, HttpStatusCode statusCode = HttpStatusCode.OK, ResponseCode responseCode = ResponseCode.Success, string message = "")
    : FuncResponse(statusCode, (Microblogging.Shared.ResponseCode)responseCode, message)
{
    public T Data { get; set; } = data;
}
