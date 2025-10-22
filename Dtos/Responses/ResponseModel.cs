namespace VaultIQ.Dtos.Responses
{
    public class ResponseModel
    {
        public bool IsSuccessful { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public int StatusCode { get; set; }

        public static ResponseModel Success(string? message = null)
        {
            return new ResponseModel()
            {
                IsSuccessful = true,
                Message = message ?? "Request was Successful",
                StatusCode = 200
            };
        }

        public static ResponseModel Failure(string? message = null) // , Dictionary<string, string> errors = null
        {
            return new ResponseModel()
            {
                Message = message ?? "Request was not completed",
                StatusCode = 400
                //Errors = errors
            };
        }
    }

    public class ResponseModel<T> : ResponseModel
    {
        public T? Data { get; set; }

        public static ResponseModel<T> Success(T data, string? message = null)
        {
            return new ResponseModel<T>()
            {
                IsSuccessful = true,
                Message = message ?? "Request was Successful",
                Data = data,
                StatusCode = 200
            };
        }

        public static new ResponseModel<T> Failure(string? message = null)
        {
            return new ResponseModel<T>()
            {
                IsSuccessful = false,
                Message = message ?? "Request was not completed",
                StatusCode = 400
            };
        }

        public static new ResponseModel<T> Exception(string? message = null)
        {
            return new ResponseModel<T>()
            {
                IsSuccessful = false,
                Message = message ?? "Request with error",
                StatusCode = 500
            };
        }
    }

    public class ResponseErrorModel : ResponseModel
    {
        public IDictionary<string, string[]>? Errors { get; set; }

        public static ResponseModel Failure(IDictionary<string, string[]>? errors = null, string? message = null)
        {
            return new ResponseErrorModel()
            {
                Message = message ?? "Request was not completed",
                Errors = errors ?? new Dictionary<string, string[]>(),
                StatusCode = 500
            };
        }
    }
}
