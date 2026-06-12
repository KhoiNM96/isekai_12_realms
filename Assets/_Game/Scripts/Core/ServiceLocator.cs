using System;
using System.Collections.Generic;

namespace Isekai12Realms.Core
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> Services = new Dictionary<Type, object>();

        public static void Register<T>(T service)
        {
            Services[typeof(T)] = service;
        }

        public static T Resolve<T>()
        {
            if (Services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }
            throw new Exception($"Service of type {typeof(T)} not registered.");
        }

        public static bool TryResolve<T>(out T service)
        {
            if (Services.TryGetValue(typeof(T), out var s))
            {
                service = (T)s;
                return true;
            }
            service = default;
            return false;
        }

        public static void Clear()
        {
            Services.Clear();
        }
    }
}
