namespace ERP.Data.DTO;

public class ResultDTO<T>
{
    // Public getters and setters for MassTransit serialization
    public bool Succeeded { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }

    private ResultDTO(bool succeeded, string message, T data)
    {
        Succeeded = succeeded;
        Message = message;
        Data = data;
    }
    // Parameterless constructor for MassTransit
    public ResultDTO() { }
    public static ResultDTO<T> Success(T data, string message = "")
    {
        return new ResultDTO<T>(true, message, data);
    }

    public static ResultDTO<T> Fail(string message, T data = default)
    {
        return new ResultDTO<T>(false, message, data);
    }
}
