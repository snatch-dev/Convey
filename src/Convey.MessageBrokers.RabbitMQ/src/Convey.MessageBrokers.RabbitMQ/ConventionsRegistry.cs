using System;
using System.Collections.Generic;

namespace Convey.MessageBrokers.RabbitMQ
{
    public class ConventionsRegistry : IConventionsRegistry
    {
        private readonly IDictionary<Type, IConventions> _conventions = new Dictionary<Type, IConventions>();

        public void Add<T>(IConventions conventions) => Add(typeof(T), conventions);
        
        public void Add(Type type, IConventions conventions) => _conventions[type] = conventions;

        public IConventions Get<T>() => Get(typeof(T));

        public IConventions Get(Type type) => _conventions.TryGetValue(type, out var conventions) ? conventions : null;

        public IEnumerable<IConventions> GetAll() => _conventions.Values;
    }
}