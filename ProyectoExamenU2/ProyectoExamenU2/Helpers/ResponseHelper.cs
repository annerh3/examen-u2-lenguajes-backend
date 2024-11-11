using ProyectoExamenU2.Dtos.Common;

namespace ProyectoExamenU2.Helpers
{
    public static class ResponseHelper
    { 
        public static ResponseDto<T> ResponseError<T>(int statusCode, string message)
        {
            return new ResponseDto<T>
            {
                StatusCode = statusCode,
                Status = false,
                Message = message
            };
        }

        public static ResponseDto<T> ResponseSuccess<T>(int statusCode, string message, T data = default)
        {
            return new ResponseDto<T>
            {
                StatusCode = statusCode,
                Status = true,
                Message = message,
                Data = data
            };
        }
    }
}