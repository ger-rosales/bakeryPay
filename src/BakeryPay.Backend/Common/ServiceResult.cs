namespace BakeryPay.Backend.Common;

public class ServiceResult<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }

    public static ServiceResult<T> Ok(T data, string message = "") =>
        new()
        {
            Success = true,
            Message = message,
            Data = data
        };

    public static ServiceResult<T> Fail(string message) =>
        new()
        {
            Success = false,
            Message = message
        };
}
