using System.Collections.Generic;
using System.Linq;

namespace MovieSpot.Services.Notifications
{
    internal static class ObjectExtensions
    {
        public static Dictionary<string, string> ToDictionaryOrEmpty(this object? data)
        {
            if (data == null) return new Dictionary<string, string>();

            return data.GetType()
                .GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(data)?.ToString() ?? "");
        }
    }
}
