namespace SyncoraBackend.Models;

public class UserRequestContext
{
    public int UserId => userId == null ? throw new InvalidOperationException("UserId value is null on user request context.") : userId.Value;
    public string DeviceId { get; private set; } = "";

    /// <summary>
    /// If the request isn't authenticated, the userId will be null
    /// </summary>
    private int? userId;



    public void PopulateContext(int? userId, string deviceId)
    {
        this.userId = userId;
        DeviceId = deviceId;


    }


    public void Print()
    {
        Console.WriteLine($"stored userId is {userId}, deviceId is {this.DeviceId}");

    }
}
