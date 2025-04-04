using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Data.Helpers;

public static class SessionHelper
{
    public static int Count { get; set; }
    public static void SetObjectAsJson(this ISession session, string key, object value)
    {
        session.SetString(key, JsonConvert.SerializeObject(value));
    }
    public static T? GetObjectFromJson<T>(this ISession session, string key) where T : class
    {
        var value = session.GetString(key);
        return value == null ? null : JsonConvert.DeserializeObject<T?>(value);
    }

}