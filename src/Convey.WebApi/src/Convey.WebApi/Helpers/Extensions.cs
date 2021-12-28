using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Convey.WebApi.Helpers;

public static class Extensions
{
    public static object GetDefaultInstance(this Type type)
    {
        if (type == typeof(string))
        {
            return string.Empty;
        }

        var defaultValueCache = new Dictionary<Type, object>();

        if (TryGetDefaultValue(type, out var instance, defaultValueCache))
        {
            return instance;
        }

        return default;
    }

    public static object SetDefaultInstanceProperties(this object instance)
        => SetDefaultInstanceProperties(instance, new Dictionary<Type, object>());

    private static object SetDefaultInstanceProperties(object instance, Dictionary<Type, object> defaultValueCache)
    {
        defaultValueCache ??= new Dictionary<Type, object>();

        var type = instance.GetType();

        foreach (var propertyInfo in type.GetProperties(BindingFlags.Instance))
        {
            if (TryGetDefaultValue(propertyInfo.PropertyType, out var defaultValue, defaultValueCache))
            {
                SetValue(propertyInfo, instance, defaultValue);
            }
        }

        return instance;
    }

    private static bool TryGetDefaultValue(Type type, out object defaultValue, Dictionary<Type, object> defaultValueCache)
    {
        if (defaultValueCache.TryGetValue(type, out defaultValue))
        {
            return true;
        }

        if (type == typeof(string))
        {
            defaultValue = string.Empty;

            defaultValueCache[type] = defaultValue;

            return true;
        }

        if (type.Name == "IDictionary`2")
        {
            defaultValue = null;

            return false;
        }

        if (typeof(IEnumerable).IsAssignableFrom(type))
        {
            if (TryGetCollectionDefaultValue(type, out defaultValue))
            {
                defaultValueCache[type] = defaultValue;

                return true;
            }

            defaultValue = null;

            return false;
        }

        if (type.IsInterface || type.IsAbstract)
        {
            defaultValue = null;

            return false;
        }

        if (type.IsArray)
        {
            if (TryGetCollectionDefaultValue(type, out defaultValue))
            {
                defaultValueCache[type] = defaultValue;

                return true;
            }

            defaultValue = null;

            return false;
        }

        if (!type.IsClass)
        {
            defaultValue = null;

            return false;
        }

        defaultValue = FormatterServices.GetUninitializedObject(type);

        defaultValueCache[type] = defaultValue;

        SetDefaultInstanceProperties(defaultValue, defaultValueCache);

        return true;
    }

    private static bool TryGetCollectionDefaultValue(Type type, out object defaultValue)
    {
        var elementType =
            type.IsGenericType
                ? type.GenericTypeArguments[0]
                : type.GetElementType();

        if (elementType is null)
        {
            defaultValue = null;

            return false;
        }

        if (typeof(IEnumerable).IsAssignableFrom(elementType))
        {
            if (elementType == typeof(string))
            {
                defaultValue = Array.Empty<string>();

                return true;
            }

            defaultValue = null;

            return false;
        }

        defaultValue = Array.CreateInstance(elementType, 0);

        return true;
    }

    private static void SetValue(PropertyInfo propertyInfo, object instance, object value)
    {
        if (propertyInfo.CanWrite)
        {
            propertyInfo.SetValue(instance, value);

            return;
        }

        var fieldInfo =
            instance
                .GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .SingleOrDefault(x => x.Name.StartsWith($"<{propertyInfo.Name}>"));

        fieldInfo?.SetValue(instance, value);
    }
}