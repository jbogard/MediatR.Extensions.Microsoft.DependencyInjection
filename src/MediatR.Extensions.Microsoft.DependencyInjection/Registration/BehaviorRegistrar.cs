using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Registration
{
    public static class BehaviorRegistrar
    {
        public static void RegisterBehaviorForCovariantRequest(
            Type assignableRequestType,
            IServiceCollection services,
            IEnumerable<Assembly> assembliesToScan)
        {
            var behaviorTypes = new List<Type>();
            var requestResponseTypes = new List<(Type requestType, Type responseType)>();

            foreach (var type in assembliesToScan.SelectMany(a => a.DefinedTypes).Where(t => t.IsConcrete()))
            {
                if (assignableRequestType.IsAssignableFrom(type))
                {
                    var requestInterfaceType =
                        GetRequestInterfaceType(type, typeof(IRequest<>)) ??
                        GetRequestInterfaceType(assignableRequestType, typeof(IRequest<>));

                    if (requestInterfaceType != null)
                    {
                        requestResponseTypes.Add((type, requestInterfaceType.GenericTypeArguments[0]));
                    }
                }

                var isTypeBehavior = type.GetInterfaces().Any(
                    x => x.IsGenericType
                         && x.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>)
                         && x.GenericTypeArguments[0] == assignableRequestType);

                if (isTypeBehavior)
                    behaviorTypes.Add(type);
            }

            foreach (var (requestType, responseType) in requestResponseTypes)
            {
                var typeArgs = new[] {requestType, responseType};
                var closedInterfaceType = typeof(IPipelineBehavior<,>).MakeGenericType(typeArgs);

                foreach (var behaviorType in behaviorTypes)
                {
                    var closedImplementationType = behaviorType.MakeGenericType(responseType);
                    services.AddTransient(closedInterfaceType, closedImplementationType);
                }
            }
        }
        private static Type GetRequestInterfaceType(Type pluggedType, Type pluginType)
        {
            return pluggedType
                .GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == pluginType);
        }
        
        private static bool IsConcrete(this Type type)
        {
            return !type.GetTypeInfo().IsAbstract && !type.GetTypeInfo().IsInterface;
        }
    }
}