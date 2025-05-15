namespace SyncoraBackend.Utilities;

public static class DateUtils
{
    public static string ExtractDateOnly(DateTime dateTime)
    {
        return dateTime.Date.ToString("yyyy/MM/dd");
    }

    public static string FormatDateAndTime(DateTime dateTime)
    {
        return dateTime.ToString("yyyy/MM/dd hh:mm:ss tt");
    }

    /// <summary>
    /// Converts a DateTime object to the format "yyyy/MM/dd hh:mm:ss tt".
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static string ToFormattedString(this DateTime dateTime) =>
        DateUtils.FormatDateAndTime(dateTime);

}