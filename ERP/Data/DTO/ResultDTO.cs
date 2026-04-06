namespace ERP.Data.DTO;

public class ResultDTO<T>
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    public static ResultDTO<T> Success(T data)
    {
        return new ResultDTO<T>
        {
            IsSuccess = true,
            Data = data
        };
    }

    public static ResultDTO<T> Failure(string message)
    {
        return new ResultDTO<T>
        {
            IsSuccess = false,
            Message = message
        };
    }
}
