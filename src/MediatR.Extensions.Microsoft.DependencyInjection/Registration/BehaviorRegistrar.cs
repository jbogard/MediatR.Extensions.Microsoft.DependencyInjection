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
            var types = assembliesToScan.SelectMany(a => a.DefinedTypes).Where(t => t.IsConcrete()).ToArray();

            var handlerTypes = types.Where(t => t.GetInterface(typeof(IRequestHandler<,>)) != null);

            var requestResponseTypes = handlerTypes.Select(h =>
                    h.GetInterface(typeof(IRequestHandler<,>)).GetGenericArguments()[0])
                .Where(assignableRequestType.IsAssignableFrom)
                .Select(request => (request, request.GetInterface(typeof(IRequest<>)).GetGenericArguments()[0]));

            var behaviorTypes = types.Where(t =>
                {
                    var @interface = t.GetInterface(typeof(IPipelineBehavior<,>));
                    return @interface?.GetGenericArguments()[0].IsAssignableFrom(assignableRequestType) ?? false;
                })
                .ToArray();

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

        private static Type GetInterface(this Type pluggedType, Type interfaceType)
        {
            return pluggedType
                .GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
        }

        private static bool IsConcrete(this Type type)
        {
            return !type.GetTypeInfo().IsAbstract && !type.GetTypeInfo().IsInterface;
        }
    }
}