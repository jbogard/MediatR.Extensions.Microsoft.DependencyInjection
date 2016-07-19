namespace MediatR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
#if DEPENDENCY_MODEL
    using Microsoft.Extensions.DependencyModel;
#endif

    public static class ServiceCollectionExtensions
    {
#if DEPENDENCY_MODEL
        public static void AddMediatR(this IServiceCollection services)
        {
            services.AddMediatR(DependencyContext.Default);
        }

        public static void AddMediatR(this IServiceCollection services, DependencyContext dependencyContext)
        {
            services.AddMediatR(dependencyContext.RuntimeLibraries
                .SelectMany(lib => lib.GetDefaultAssemblyNames(dependencyContext).Select(Assembly.Load)));
        }
#endif

        public static void AddMediatR(this IServiceCollection services, params Assembly[] assemblies)
        {
            AddRequiredServices(services);

            AddMediatRClasses(services, assemblies);
        }

        public static void AddMediatR(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            AddRequiredServices(services);

            AddMediatRClasses(services, assemblies);
        }

        public static void AddMediatR(this IServiceCollection services, params Type[] handlerAssemblyMarkerTypes)
        {
            AddRequiredServices(services);

            AddMediatRClasses(services, handlerAssemblyMarkerTypes.Select(t => t.GetTypeInfo().Assembly));
        }

        public static void AddMediatR(this IServiceCollection services, IEnumerable<Type> handlerAssemblyMarkerTypes)
        {
            AddRequiredServices(services);
            AddMediatRClasses(services, handlerAssemblyMarkerTypes.Select(t => t.GetTypeInfo().Assembly));
        }


        private static void AddMediatRClasses(IServiceCollection services, IEnumerable<Assembly> assembliesToScan)
        {
            assembliesToScan = assembliesToScan as Assembly[] ?? assembliesToScan.ToArray();

            var openInterfaces = new[]
            {
                typeof(IRequestHandler<,>),
                typeof(IAsyncRequestHandler<,>),
                typeof(ICancellableAsyncRequestHandler<,>),
                typeof(INotificationHandler<>),
                typeof(IAsyncNotificationHandler<>),
                typeof(ICancellableAsyncNotificationHandler<>)
            };
            foreach (var openInterface in openInterfaces)
            {
                var concretions = new List<Type>();
                var interfaces = new List<Type>();

                foreach (var type in assembliesToScan.SelectMany(a => a.ExportedTypes))
                {
                    IEnumerable<Type> interfaceTypes = type.FindInterfacesThatClose(openInterface).ToArray();
                    if (!interfaceTypes.Any()) continue;

                    if (type.IsConcrete())
                    {
                        concretions.Add(type);
                    }

                    foreach (Type interfaceType in interfaceTypes)
                    {
                        interfaces.Fill(interfaceType);
                    }
                }

                foreach (var @interface in interfaces)
                {
                    var exactMatches = concretions.Where(t => t.CanBeCastTo(@interface)).ToArray();

                    foreach (var exactMatch in exactMatches)
                    {
                        services.AddTransient(@interface, exactMatch);
                    }

                    if (!@interface.IsOpenGeneric())
                    {
                        AddConcretionsThatCouldBeClosed(@interface, concretions, services);
                    }
                }
            }
        }

        private static void AddConcretionsThatCouldBeClosed(Type @interface, List<Type> concretions, IServiceCollection services)
        {
            foreach (var type in concretions.Where(x => x.IsOpenGeneric())
                .Where(x => x.CouldCloseTo(@interface)))
            {
                try
                {
                    services.AddTransient(@interface, type.MakeGenericType(@interface.GenericTypeArguments));
                }
                catch (Exception)
                {
                }
            }
        }

        private static bool CouldCloseTo(this Type openConcretion, Type closedInterface)
        {
            var openInterface = closedInterface.GetGenericTypeDefinition();
            var arguments = closedInterface.GenericTypeArguments;

            var concreteArguments = openConcretion.GenericTypeArguments;
            return arguments.Length == concreteArguments.Length && openConcretion.CanBeCastTo(openInterface);
        }

        private static bool CanBeCastTo(this Type pluggedType, Type pluginType)
        {
            if (pluggedType == null) return false;

            if (pluggedType == pluginType) return true;

            return pluginType.GetTypeInfo().IsAssignableFrom(pluggedType.GetTypeInfo());
        }

        public static bool IsOpenGeneric(this Type type)
        {
            return type.GetTypeInfo().IsGenericTypeDefinition || type.GetTypeInfo().ContainsGenericParameters;
        }

        private static IEnumerable<Type> FindInterfacesThatClose(this Type pluggedType, Type templateType)
        {
            if (!pluggedType.IsConcrete()) yield break;

            if (templateType.GetTypeInfo().IsInterface)
            {
                foreach (
                    var interfaceType in
                        pluggedType.GetTypeInfo().ImplementedInterfaces
                            .Where(type => type.GetTypeInfo().IsGenericType && (type.GetGenericTypeDefinition() == templateType)))
                {
                    yield return interfaceType;
                }
            }
            else if (pluggedType.GetTypeInfo().BaseType.GetTypeInfo().IsGenericType &&
                     (pluggedType.GetTypeInfo().BaseType.GetGenericTypeDefinition() == templateType))
            {
                yield return pluggedType.GetTypeInfo().BaseType;
            }

            if (pluggedType.GetTypeInfo().BaseType == typeof(object)) yield break;

            foreach (var interfaceType in FindInterfacesThatClose(pluggedType.GetTypeInfo().BaseType, templateType))
            {
                yield return interfaceType;
            }
        }

        private static bool IsConcrete(this Type type)
        {
            return !type.GetTypeInfo().IsAbstract && !type.GetTypeInfo().IsInterface;
        }

        private static void Fill<T>(this IList<T> list, T value)
        {
            if (list.Contains(value)) return;
            list.Add(value);
        }


        private static void AddRequiredServices(IServiceCollection services)
        {
            services.AddScoped<SingleInstanceFactory>(p => t => p.GetRequiredService(t));
            services.AddScoped<MultiInstanceFactory>(p => t => p.GetRequiredServices(t));
            services.AddSingleton<IMediator, Mediator>();
        }

        private static IEnumerable<object> GetRequiredServices(this IServiceProvider provider, Type serviceType)
        {
            return (IEnumerable<object>)provider.GetRequiredService(typeof(IEnumerable<>).MakeGenericType(serviceType));
        }
    }
}