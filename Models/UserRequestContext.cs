namespace SyncoraBackend.Models;

public class UserRequestContext
{
    /// <summary>
    /// If the request isn't authenticated, the userId will be null
    /// </summary>
    public int? UserId { get; private set; }
    public string DeviceId { get; private set; } = "";


    public void PopulateContext(int? userId, string deviceId)
    {
        UserId = userId;
        DeviceId = deviceId;
    }
}
