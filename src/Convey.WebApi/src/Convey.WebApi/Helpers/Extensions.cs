using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Convey.WebApi.Helpers
{
    public static class Extensions
    {
        public static object SetDefaultInstanceProperties(this object instance)
        {
            var type = instance.GetType();
            foreach (var propertyInfo in type.GetProperties())
            {
                SetValue(propertyInfo, instance);
            }

            return instance;
        }

        private static void SetValue(PropertyInfo propertyInfo, object instance)
        {
            var propertyType = propertyInfo.PropertyType;
            if (propertyType == typeof(string))
            {
                SetDefaultValue(propertyInfo, instance, string.Empty);
                return;
            }
            
            if (propertyType.Name == "IDictionary`2")
            {
                return;
            }

            if (typeof(IEnumerable).IsAssignableFrom(propertyType))
            {
                SetCollection(propertyInfo, instance);

                return;
            }

            if (propertyType.IsInterface)
            {
                return;
            }

            if (propertyType.IsArray)
            {
                SetCollection(propertyInfo, instance);
                return;
            }

            if (!propertyType.IsClass)
            {
                return;
            }

            var propertyInstance = FormatterServices.GetUninitializedObject(propertyInfo.PropertyType);
            SetDefaultValue(propertyInfo, instance, propertyInstance);
            SetDefaultInstanceProperties(propertyInstance);
        }

        private static void SetCollection(PropertyInfo propertyInfo, object instance)
        {
            var elementType = propertyInfo.PropertyType.IsGenericType
                ? propertyInfo.PropertyType.GenericTypeArguments[0]
                : propertyInfo.PropertyType.GetElementType();
            if (elementType is null)
            {
                return;
            }

            if (typeof(IEnumerable).IsAssignableFrom(elementType))
            {
                if (elementType == typeof(string))
                {
                    SetDefaultValue(propertyInfo, instance, Array.Empty<string>());
                    return;
                }
                
                return;
            }

            var array = Array.CreateInstance(elementType, 0);
            SetDefaultValue(propertyInfo, instance, array);
        }
        
        private static void SetDefaultValue(PropertyInfo propertyInfo, object instance, object value)
        {
            if (propertyInfo.CanWrite)
            {
                propertyInfo.SetValue(instance, value);
                return;
            }

            var propertyName = propertyInfo.Name;
            var field = instance.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .SingleOrDefault(x => x.Name.StartsWith($"<{propertyName}>"));
            field?.SetValue(instance, value);
        }
    }
}