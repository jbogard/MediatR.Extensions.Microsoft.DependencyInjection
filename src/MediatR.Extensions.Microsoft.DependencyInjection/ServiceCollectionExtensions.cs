namespace MediatR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Pipeline;
#if DEPENDENCY_MODEL
    using Microsoft.Extensions.DependencyModel;
#endif

    /// <summary>
    /// Extensions to scan for MediatR handlers and registers them.
    /// - Scans for any handler interface implementations and registers them as <see cref="ServiceLifetime.Transient"/>
    /// - Scans for any <see cref="IRequestPreProcessor{TRequest}"/> and <see cref="IRequestPostProcessor{TRequest,TResponse}"/> implementations and registers them as scoped instances
    /// Registers <see cref="SingleInstanceFactory"/>, <see cref="MultiInstanceFactory"/> and <see cref="IMediator"/> as scoped instances
    /// After calling AddMediatR you can use the container to resolve an <see cref="IMediator"/> instance.
    /// This does not scan for any <see cref="IPipelineBehavior{TRequest,TResponse}"/> instances including <see cref="RequestPreProcessorBehavior{TRequest,TResponse}"/> and <see cref="RequestPreProcessorBehavior{TRequest,TResponse}"/>.
    /// To register behaviors, use the <see cref="ServiceCollectionServiceExtensions.AddTransient(IServiceCollection,Type,Type)"/> with the open generic or closed generic types.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
#if DEPENDENCY_MODEL
        private static HashSet<string> ReferenceAssemblies { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "MediatR"
        };

        /// <summary>
        /// Registers handlers and the mediator types from <see cref="DependencyContext.Default"/>.
        /// </summary>
        /// <param name="services">Service collection</param>
        public static void AddMediatR(this IServiceCollection services)
        {
            services.AddMediatR(DependencyContext.Default);
        }

        /// <summary>
        /// Registers handlers and mediator types from the specified <see cref="DependencyContext"/>
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="dependencyContext"></param>
        public static void AddMediatR(this IServiceCollection services, DependencyContext dependencyContext)
        {
            services.AddMediatR(GetCandidateAssemblies(dependencyContext));
        }

        private static IEnumerable<Assembly> GetCandidateAssemblies(DependencyContext dependencyContext)
        {
            return GetCandidateLibraries(dependencyContext)
                .SelectMany(library => library.GetDefaultAssemblyNames(dependencyContext))
                .Select(Assembly.Load);
        }

        private static IEnumerable<RuntimeLibrary> GetCandidateLibraries(DependencyContext dependencyContext)
        {
            if (ReferenceAssemblies == null)
            {
                return Enumerable.Empty<RuntimeLibrary>();
            }

            var candidatesResolver = new CandidateResolver(dependencyContext.RuntimeLibraries, ReferenceAssemblies);
            return candidatesResolver.GetCandidates();
        }

        private class CandidateResolver
        {
            private readonly IDictionary<string, Dependency> _dependencies;

            public CandidateResolver(IReadOnlyList<RuntimeLibrary> dependencies, ISet<string> referenceAssemblies)
            {
                var dependenciesWithNoDuplicates = new Dictionary<string, Dependency>(StringComparer.OrdinalIgnoreCase);
                foreach (var dependency in dependencies)
                {
                    if (dependenciesWithNoDuplicates.ContainsKey(dependency.Name))
                    {
                        throw new InvalidOperationException(
                            $"A duplicate entry for library reference {dependency.Name} was found. Please check that all package references in all projects use the same casing for the same package references.");
                    }
                    dependenciesWithNoDuplicates.Add(dependency.Name, CreateDependency(dependency, referenceAssemblies));
                }

                _dependencies = dependenciesWithNoDuplicates;
            }

            private Dependency CreateDependency(RuntimeLibrary library, ISet<string> referenceAssemblies)
            {
                var classification = DependencyClassification.Unknown;
                if (referenceAssemblies.Contains(library.Name))
                {
                    classification = DependencyClassification.MediatRReference;
                }

                return new Dependency(library, classification);
            }

            private DependencyClassification ComputeClassification(string dependency)
            {
                var candidateEntry = _dependencies[dependency];
                if (candidateEntry.Classification != DependencyClassification.Unknown)
                {
                    return candidateEntry.Classification;
                }
                else
                {
                    var classification = DependencyClassification.NotCandidate;
                    foreach (var candidateDependency in candidateEntry.Library.Dependencies)
                    {
                        var dependencyClassification = ComputeClassification(candidateDependency.Name);
                        if (dependencyClassification == DependencyClassification.Candidate ||
                            dependencyClassification == DependencyClassification.MediatRReference)
                        {
                            classification = DependencyClassification.Candidate;
                            break;
                        }
                    }

                    candidateEntry.Classification = classification;

                    return classification;
                }
            }

            public IEnumerable<RuntimeLibrary> GetCandidates()
            {
                foreach (var dependency in _dependencies)
                {
                    if (ComputeClassification(dependency.Key) == DependencyClassification.Candidate)
                    {
                        yield return dependency.Value.Library;
                    }
                }
            }

            private class Dependency
            {
                public Dependency(RuntimeLibrary library, DependencyClassification classification)
                {
                    Library = library;
                    Classification = classification;
                }

                public RuntimeLibrary Library { get; }

                public DependencyClassification Classification { get; set; }

                public override string ToString()
                {
                    return $"Library: {Library.Name}, Classification: {Classification}";
                }
            }

            private enum DependencyClassification
            {
                Unknown = 0,
                Candidate = 1,
                NotCandidate = 2,
                MediatRReference = 3
            }
        }
#endif

        /// <summary>
        /// Registers handlers and mediator types from the specified assemblies
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="assemblies">Assemblies to scan</param>
        public static void AddMediatR(this IServiceCollection services, params Assembly[] assemblies)
        {
            AddRequiredServices(services);

            AddMediatRClasses(services, assemblies);
        }

        /// <summary>
        /// Registers handlers and mediator types from the specified assemblies
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="assemblies">Assemblies to scan</param>
        public static void AddMediatR(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            AddRequiredServices(services);

            AddMediatRClasses(services, assemblies);
        }

        /// <summary>
        /// Registers handlers and mediator types from the assemblies that contain the specified types
        /// </summary>
        /// <param name="services"></param>
        /// <param name="handlerAssemblyMarkerTypes"></param>
        public static void AddMediatR(this IServiceCollection services, params Type[] handlerAssemblyMarkerTypes)
        {
            AddRequiredServices(services);

            AddMediatRClasses(services, handlerAssemblyMarkerTypes.Select(t => t.GetTypeInfo().Assembly));
        }

        /// <summary>
        /// Registers handlers and mediator types from the assemblies that contain the specified types
        /// </summary>
        /// <param name="services"></param>
        /// <param name="handlerAssemblyMarkerTypes"></param>
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
                typeof(IRequestHandler<>),
                typeof(IAsyncRequestHandler<,>),
                typeof(IAsyncRequestHandler<>),
                typeof(ICancellableAsyncRequestHandler<,>),
                typeof(ICancellableAsyncRequestHandler<>),
                typeof(INotificationHandler<>),
                typeof(IAsyncNotificationHandler<>),
                typeof(ICancellableAsyncNotificationHandler<>),
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

            var multiOpenInterfaces = new[]
            {
                typeof(IRequestPreProcessor<>),
                typeof(IRequestPostProcessor<,>)
            };

            foreach (var multiOpenInterface in multiOpenInterfaces)
            {
                var concretions = new List<Type>();

                foreach (var type in assembliesToScan.SelectMany(a => a.ExportedTypes))
                {
                    IEnumerable<Type> interfaceTypes = type.FindInterfacesThatClose(multiOpenInterface).ToArray();
                    if (!interfaceTypes.Any()) continue;

                    if (type.IsConcrete())
                    {
                        concretions.Add(type);
                    }
                }

                foreach (var concretion in concretions)
                {
                    services.AddTransient(multiOpenInterface, concretion);
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
            services.AddScoped<IMediator, Mediator>();
        }

        private static IEnumerable<object> GetRequiredServices(this IServiceProvider provider, Type serviceType)
        {
            return (IEnumerable<object>)provider.GetRequiredService(typeof(IEnumerable<>).MakeGenericType(serviceType));
        }
    }
}