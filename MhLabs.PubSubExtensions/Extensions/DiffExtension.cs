using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

static class DiffExtension
{
    public static List<string> PropertyDiff<T>(this T val1, T val2, string prefix = null, bool indexed = false) where T : class, new()
    {
        List<string> variances = new List<string>();
        var nullSafeValue = (val1 ?? val2);
        var pi = nullSafeValue.GetType().GetProperties().Where(x => !x.GetIndexParameters().Any());
        foreach (var p in pi)
        {
            var value1 = val1 != null ? p.GetValue(val1) : null;
            var value2 = val2 != null ? p.GetValue(val2) : null;
            if ((value1 == null ^ value2 == null) || !value1?.Equals(value2) == true)
            {
                var fullName = GetFullName(prefix, p.Name);
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
        var piIndexed = nullSafeValue.GetType().GetProperties().Any(x => x.GetIndexParameters().Length > 0);
        if (piIndexed)
        {
            if (nullSafeValue is IDictionary)
            {
                var dict1 = (IDictionary)val1;
                var dict2 = (IDictionary)val2;
                foreach (var key in (dict1 ?? dict2).Keys)
                {
                    var obj1 = dict1 != null && dict1.Contains(key) ? dict1[key] : null;
                    var obj2 = dict2 != null && dict2.Contains(key) ? dict2[key] : null;
                    return obj1.PropertyDiff(obj2, prefix);
                }

            }
            else
            {
                var enum1 = (IEnumerable<object>)val1;
                var enum2 = (IEnumerable<object>)val2;
                for (var i = 0; i < (enum1 ?? enum2).Count(); i++)
                {
                    var obj1 = enum1 != null ? enum1.ElementAtOrDefault(i) : null;
                    var obj2 = enum2 != null ? enum2.ElementAtOrDefault(i) : null;
                    return obj1.PropertyDiff(obj2, prefix);
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
