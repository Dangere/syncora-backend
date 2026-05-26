namespace SyncoraBackend.Models;

public class UserRequestContext
{
    /// <summary>
    /// If the request isn't authenticated, the UserId will throw 
    /// </summary>
    public int UserId => userId ?? throw new InvalidOperationException("Unauthenticated user.");

    /// <summary>
    /// If the request isn't authenticated, the DeviceId will throw
    /// </summary>
    public string DeviceId => deviceId ?? throw new InvalidOperationException("Unauthenticated user.");

    private int? userId;
    private string? deviceId;




    public void PopulateContext(int? userId, string deviceId)
    {
        this.userId = userId;
        this.deviceId = deviceId;
    }
}
