using System.Collections;

namespace MediatR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Pipeline;

    /// <summary>
    /// Extensions to scan for MediatR handlers and registers them.
    /// - Scans for any handler interface implementations and registers them as <see cref="ServiceLifetime.Transient"/>
    /// - Scans for any <see cref="IRequestPreProcessor{TRequest}"/> and <see cref="IRequestPostProcessor{TRequest,TResponse}"/> implementations and registers them as scoped instances
    /// Registers <see cref="ServiceFactory"/> and <see cref="IMediator"/> as scoped instances
    /// After calling AddMediatR you can use the container to resolve an <see cref="IMediator"/> instance.
    /// This does not scan for any <see cref="IPipelineBehavior{TRequest,TResponse}"/> instances including <see cref="RequestPreProcessorBehavior{TRequest,TResponse}"/> and <see cref="RequestPreProcessorBehavior{TRequest,TResponse}"/>.
    /// To register behaviors, use the <see cref="ServiceCollectionServiceExtensions.AddTransient(IServiceCollection,Type,Type)"/> with the open generic or closed generic types.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers handlers and the mediator types from <see cref="AppDomain.CurrentDomain"/>.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddMediatR(this IServiceCollection services)
            => services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic));

        /// <summary>
        /// Registers handlers and mediator types from the specified assemblies
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="assemblies">Assemblies to scan</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddMediatR(this IServiceCollection services, params Assembly[] assemblies)
            => services.AddMediatR(assemblies.AsEnumerable());

        /// <summary>
        /// Registers handlers and mediator types from the specified assemblies
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="assemblies">Assemblies to scan</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddMediatR(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            AddRequiredServices(services);

            AddMediatRClasses(services, assemblies);

            return services;
        }

        /// <summary>
        /// Registers handlers and mediator types from the assemblies that contain the specified types
        /// </summary>
        /// <param name="services"></param>
        /// <param name="handlerAssemblyMarkerTypes"></param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddMediatR(this IServiceCollection services, params Type[] handlerAssemblyMarkerTypes)
            => services.AddMediatR(handlerAssemblyMarkerTypes.AsEnumerable());

        /// <summary>
        /// Registers handlers and mediator types from the assemblies that contain the specified types
        /// </summary>
        /// <param name="services"></param>
        /// <param name="handlerAssemblyMarkerTypes"></param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddMediatR(this IServiceCollection services, IEnumerable<Type> handlerAssemblyMarkerTypes)
        {
            AddRequiredServices(services);
            AddMediatRClasses(services, handlerAssemblyMarkerTypes.Select(t => t.GetTypeInfo().Assembly));
            return services;
        }

        private static void AddMediatRClasses(IServiceCollection services, IEnumerable<Assembly> assembliesToScan)
        {
            assembliesToScan = (assembliesToScan as Assembly[] ?? assembliesToScan).Distinct().ToArray();

            var openRequestInterfaces = new[]
            {
                typeof(IRequestHandler<,>),
            };
            var openNotificationHandlerInterfaces = new[]
            {
                typeof(INotificationHandler<>),
            };
            AddInterfacesAsTransient(openRequestInterfaces, services, assembliesToScan, false);
            AddInterfacesAsTransient(openNotificationHandlerInterfaces, services, assembliesToScan, true);

            var multiOpenInterfaces = new[]
            {
                typeof(IRequestPreProcessor<>),
                typeof(IRequestPostProcessor<,>)
            };

            foreach (var multiOpenInterface in multiOpenInterfaces)
            {
                var concretions = new List<Type>();

                foreach (var type in assembliesToScan.SelectMany(a => a.DefinedTypes))
                {
                    IEnumerable<Type> interfaceTypes = type.FindInterfacesThatClose(multiOpenInterface).ToArray();
                    if (!interfaceTypes.Any()) continue;

                    if (type.IsConcrete())
                    {
                        concretions.Add(type);
                    }
                }

                // Always add every pre/post processor
                concretions
                    .ForEach(c => services.AddTransient(multiOpenInterface, c));
            }
        }

        /// <summary>
        /// Helper method use to differentiate behavior between request handlers and notification handlers.
        /// Request handlers should only be added once (so set addIfAlreadyExists to false)
        /// Notification handlers should all be added (set addIfAlreadyExists to true)
        /// </summary>
        /// <param name="openRequestInterfaces"></param>
        /// <param name="services"></param>
        /// <param name="assembliesToScan"></param>
        /// <param name="addIfAlreadyExists"></param>
        private static void AddInterfacesAsTransient(Type[] openRequestInterfaces,
            IServiceCollection services,
            IEnumerable<Assembly> assembliesToScan,
            bool addIfAlreadyExists)
        {
            foreach (var openInterface in openRequestInterfaces)
            {
                var concretions = new List<Type>();
                var interfaces = new List<Type>();

                foreach (var type in assembliesToScan.SelectMany(a => a.DefinedTypes))
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
                    var matches = concretions
                        .Where(t => t.CanBeCastTo(@interface))
                        .ToList();

                    if (addIfAlreadyExists)
                    {
                        matches.ForEach(match => services.AddTransient(@interface, match));
                    }
                    else
                    {
                        if (matches.Count() > 1)
                        {
                            matches.RemoveAll(m => !IsMatchingWithInterface(m, @interface));
                        }

                        matches.ForEach(match => services.TryAddTransient(@interface, match));
                    }

                    if (!@interface.IsOpenGeneric())
                    {
                        AddConcretionsThatCouldBeClosed(@interface, concretions, services);
                    }
                }
            }
        }

        private static bool IsMatchingWithInterface(Type handlerType, Type handlerInterface)
        {
            if (handlerType == null || handlerInterface == null)
            {
                return false;
            }

            if (handlerType.IsInterface)
            {
                if (handlerType.GenericTypeArguments.SequenceEqual(handlerInterface.GenericTypeArguments))
                {
                    return true;
                }
            }
            else
            {
                return IsMatchingWithInterface(handlerType.GetInterface(handlerInterface.Name), handlerInterface);
            }

            return false;
        }

        private static void AddConcretionsThatCouldBeClosed(Type @interface, List<Type> concretions, IServiceCollection services)
        {
            foreach (var type in concretions
                .Where(x => x.IsOpenGeneric() && x.CouldCloseTo(@interface)))
            {
                try
                {
                    services.TryAddTransient(@interface, type.MakeGenericType(@interface.GenericTypeArguments));
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

            if (pluggedType == typeof(object)) yield break;
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
            services.AddScoped<ServiceFactory>(p => (type =>
            {
                try
                {
                    return p.GetService(type);
                }
                catch (ArgumentException)
                {
                    // Let's assume it's a constrained generic type
                    if (type.IsConstructedGenericType &&
                        type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    {
                        var serviceType = type.GenericTypeArguments.Single();
                        var serviceTypes = new List<Type>();
                        foreach (var service in services.ToList())
                        {
                            if (serviceType.IsConstructedGenericType &&
                                serviceType.GetGenericTypeDefinition() == service.ServiceType)
                            {
                                try
                                {
                                    var closedImplType = service.ImplementationType.MakeGenericType(serviceType.GenericTypeArguments);
                                    serviceTypes.Add(closedImplType);
                                } catch { }
                            }
                        }

                        services.Replace(new ServiceDescriptor(type, sp =>
                        {
                            return serviceTypes.Select(sp.GetService).ToArray();
                        }, ServiceLifetime.Transient));

                        var resolved = Array.CreateInstance(serviceType, serviceTypes.Count);
                        
                        Array.Copy(serviceTypes.Select(p.GetService).ToArray(), resolved, serviceTypes.Count);

                        return resolved;
                    }

                    throw;
                }
            }));
            services.AddScoped<IMediator, Mediator>();
        }
    }
}
