using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

static class DiffExtension
{
    public static List<string> PropertyDiff<T>(this T val1, T val2, string prefix = null) where T : class, new()
    {
        List<string> variances = new List<string>();
        var pi = (val1 ?? val2).GetType().GetProperties();
        foreach (var p in pi)
        {
            var value1 = val1 != null ? p.GetValue(val1) : null;
            var value2 = val2 != null ? p.GetValue(val2) : null;
            if ((value1 == null ^ value2 == null )  || !value1?.Equals(value2) == true)
            {
                string fullName = GetFullName(prefix, p.Name);
                if (!IsSimple(p.PropertyType))
                {
                    var count = variances.Count;
                    variances.AddRange(value1.PropertyDiff(value2, fullName).Select(q => prefix + q));
                    if (variances.Count > count)
                    {
                        variances.Add(fullName);
                    }
                }
                else
                {
                    variances.Add(fullName);
                }
            }
        }
        return variances;
    }

    private static string GetFullName(string prefix, string name)
    {
        if (!string.IsNullOrEmpty(prefix))
        {
            return prefix + "." + name;
        }
        return name;
    }

    private static bool IsSimple(Type type)
    {
        return type.IsPrimitive
          || type.IsEnum
          || type.Equals(typeof(string))
          || type.Equals(typeof(decimal))
          || type.Equals(typeof(DateTime));
    }
}
