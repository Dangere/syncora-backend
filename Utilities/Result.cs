
namespace LibraryManagementSystem.Utilities;

//This class is used to return a value and an error message if there is one
//T is the type of the value that is returned
//When theres an error T would be returned null if the data type is a reference type
//When theres an error T would be returned default if the data type is a value type
public class Result<T>(T? data, string? errorMessage = null)
{
    public T? Data = data;
    public bool IsSuccess => ErrorMessage == null && Data != null;
    public string? ErrorMessage => errorMessage;

    public static Result<T> Success(T data)
    {
        return new Result<T>(data);
    }

    public static Result<T> Error(string errorMessage)
    {
        return new Result<T>(default, errorMessage);
    }
}
