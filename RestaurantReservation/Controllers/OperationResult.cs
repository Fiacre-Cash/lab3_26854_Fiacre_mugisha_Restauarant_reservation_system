namespace RestaurantReservation.Controllers;

/// <summary>Result returned by controllers so views can show a message without catching exceptions.</summary>
public class OperationResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = "";
    public static OperationResult Ok(string message = "") => new() { Success = true, Message = message };
    public static OperationResult Fail(string message) => new() { Success = false, Message = message };
}

public class OperationResult<T> : OperationResult
{
    public T? Value { get; init; }
    public static OperationResult<T> Ok(T value, string message = "") => new() { Success = true, Value = value, Message = message };
    public new static OperationResult<T> Fail(string message) => new() { Success = false, Message = message };
}
