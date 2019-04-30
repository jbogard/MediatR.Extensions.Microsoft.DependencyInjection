namespace MediatR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Pipeline;
    using Registration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extensions to scan for MediatR handlers and registers them.
    /// - Scans for any handler interface implementations and registers them as <see cref="ServiceLifetime.Transient"/>
    /// - Scans for any <see cref="IRequestPreProcessor{TRequest}"/> and <see cref="IRequestPostProcessor{TRequest,TResponse}"/> implementations and registers them as transient instances
    /// Registers <see cref="ServiceFactory"/> and <see cref="IMediator"/> as transient instances
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
            => services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic), configuration: null);

        /// <summary>
        /// Registers handlers and the mediator types from <see cref="AppDomain.CurrentDomain"/>.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">The action used to configure the options</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddMediatR(this IServiceCollection services, Action<MediatRServiceConfiguration> configuration)
            => services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic), configuration);

        /// <summary>
        /// Registers handlers and mediator types from the specified assemblies
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="assemblies">Assemblies to scan</param>        
        /// <returns>Service collection</returns>
        public static IServiceCollection AddMediatR(this IServiceCollection services, params Assembly[] assemblies)
            => services.AddMediatR(assemblies.AsEnumerable(), configuration: null);

        /// <summary>
        /// Registers handlers and mediator types from the specified assemblies
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="assemblies">Assemblies to scan</param>
        /// <param name="configuration">The action used to configure the options</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddMediatR(this IServiceCollection services, Action<MediatRServiceConfiguration> configuration, params Assembly[] assemblies)
            => services.AddMediatR(assemblies.AsEnumerable(), configuration);

        /// <summary>
        /// Registers handlers and mediator types from the specified assemblies
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="assemblies">Assemblies to scan</param>
        /// <param name="configuration">The action used to configure the options</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddMediatR(this IServiceCollection services, IEnumerable<Assembly> assemblies, Action<MediatRServiceConfiguration> configuration)
        {
            var serviceConfig = new MediatRServiceConfiguration();

            configuration?.Invoke(serviceConfig);

            ServiceRegistrar.AddRequiredServices(services, serviceConfig);

            ServiceRegistrar.AddMediatRClasses(services, assemblies);

            return services;
        }

        /// <summary>
        /// Registers handlers and mediator types from the assemblies that contain the specified types
        /// </summary>
        /// <param name="services"></param>
        /// <param name="handlerAssemblyMarkerTypes"></param>        
        /// <returns>Service collection</returns>
        public static IServiceCollection AddMediatR(this IServiceCollection services, params Type[] handlerAssemblyMarkerTypes)
            => services.AddMediatR(handlerAssemblyMarkerTypes.AsEnumerable(), configuration: null);
        
        /// <summary>
        /// Registers handlers and mediator types from the assemblies that contain the specified types
        /// </summary>
        /// <param name="services"></param>
        /// <param name="handlerAssemblyMarkerTypes"></param>
        /// <param name="configuration">The action used to configure the options</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddMediatR(this IServiceCollection services, Action<MediatRServiceConfiguration> configuration, params Type[] handlerAssemblyMarkerTypes)
            => services.AddMediatR(handlerAssemblyMarkerTypes.AsEnumerable(), configuration);

        /// <summary>
        /// Registers handlers and mediator types from the assemblies that contain the specified types
        /// </summary>
        /// <param name="services"></param>
        /// <param name="handlerAssemblyMarkerTypes"></param>
        /// <param name="configuration">The action used to configure the options</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddMediatR(this IServiceCollection services, IEnumerable<Type> handlerAssemblyMarkerTypes, Action<MediatRServiceConfiguration> configuration)
        {
            var serviceConfig = new MediatRServiceConfiguration();
            configuration?.Invoke(serviceConfig);
            ServiceRegistrar.AddRequiredServices(services, serviceConfig);
            ServiceRegistrar.AddMediatRClasses(services, handlerAssemblyMarkerTypes.Select(t => t.GetTypeInfo().Assembly));
            return services;
        }
    }
}
