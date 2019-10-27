using System.Collections.Generic;
using System.Reflection;
using Utf8Json;
using Utf8Json.Resolvers;

namespace Convey.WebApi
{
    internal sealed class ConveyFormatterResolver : IJsonFormatterResolver
    {
        public static readonly IJsonFormatterResolver Instance = new ConveyFormatterResolver();

        private static readonly IJsonFormatterResolver[] Resolvers =
        {
            StandardResolver.AllowPrivateCamelCase,
            
        };

        public IJsonFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }

        public static List<IJsonFormatter> Formatters { get; } = new List<IJsonFormatter>();

        private static class FormatterCache<T>
        {
            public static readonly IJsonFormatter<T> Formatter;

            static FormatterCache()
            {
                foreach (var item in Formatters)
                {
                    foreach (var implInterface in item.GetType().GetTypeInfo().ImplementedInterfaces)
                    {
                        var ti = implInterface.GetTypeInfo();
                        if (ti.IsGenericType && ti.GenericTypeArguments[0] == typeof(T))
                        {
                            Formatter = (IJsonFormatter<T>) item;
                            return;
                        }
                    }
                }

                foreach (var item in Resolvers)
                {
                    var formatter = item.GetFormatter<T>();
                    if (formatter is null)
                    {
                        continue;
                    }

                    Formatter = formatter;
                    return;
                }
            }
        }
    }
}