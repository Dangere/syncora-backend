namespace SyncoraBackend.Utilities;

//This class is used to return a value and an error message if there is one
//T is the type of the value that is returned
//When theres an error T would be returned null if the data type is a reference type
//When theres an error T would be returned default if the data type is a value type
public class Result<T>(T? data, string? errorMessage = null, int errorStatusCode = StatusCodes.Status400BadRequest, ErrorCodes errorCode = ErrorCodes.INTERNAL_ERROR)
{
    public T? Data = data;
    public bool IsSuccess => ErrorMessage == null && Data != null;
    public string? ErrorMessage => errorMessage;
    public int ErrorStatusCode => errorStatusCode;
    public string ErrorCode => errorCode.ToString();

    public static Result<T> Success(T data)
    {
        return new Result<T>(data, null);
    }

    public static Result<T> Error(string errorMessage, ErrorCodes errorCode, int errorStatusCode = StatusCodes.Status400BadRequest)
    {
        return new Result<T>(default, errorMessage, errorStatusCode, errorCode);
    }

    public static Result<T> ErrorFrom<TSource>(Result<TSource> result)
    {
        return Error(result.ErrorMessage!, Enum.Parse<ErrorCodes>(result.ErrorCode), errorStatusCode: result.ErrorStatusCode);
    }
}
