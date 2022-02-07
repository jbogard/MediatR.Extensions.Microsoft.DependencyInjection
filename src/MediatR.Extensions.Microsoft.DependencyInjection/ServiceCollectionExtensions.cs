using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MediatR.Pipeline;
using MediatR.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR;

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
    /// Registers handlers and mediator types from the specified assemblies
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="assemblies">Assemblies to scan</param>        
    /// <returns>Service collection</returns>
    public static IServiceCollection AddMediatR(this IServiceCollection services, params Assembly[] assemblies)
        => services.AddMediatR(assemblies, configuration: null);

    /// <summary>
    /// Registers handlers and mediator types from the specified assemblies
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="assemblies">Assemblies to scan</param>
    /// <param name="configuration">The action used to configure the options</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddMediatR(this IServiceCollection services, Action<MediatRServiceConfiguration>? configuration, params Assembly[] assemblies)
        => services.AddMediatR(assemblies, configuration);

    /// <summary>
    /// Registers handlers and mediator types from the specified assemblies
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="assemblies">Assemblies to scan</param>
    /// <param name="configuration">The action used to configure the options</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddMediatR(this IServiceCollection services, IEnumerable<Assembly> assemblies, Action<MediatRServiceConfiguration>? configuration)
    {
        if (!assemblies.Any())
        {
            throw new ArgumentException("No assemblies found to scan. Supply at least one assembly to scan for handlers.");
        }
        var serviceConfig = new MediatRServiceConfiguration();

        configuration?.Invoke(serviceConfig);

        ServiceRegistrar.AddRequiredServices(services, serviceConfig);

        ServiceRegistrar.AddMediatRClasses(services, assemblies, serviceConfig);

        return services;
    }

    /// <summary>
    /// Registers handlers and mediator types from the assemblies that contain the specified types
    /// </summary>
    /// <param name="services"></param>
    /// <param name="handlerAssemblyMarkerTypes"></param>        
    /// <returns>Service collection</returns>
    public static IServiceCollection AddMediatR(this IServiceCollection services, params Type[] handlerAssemblyMarkerTypes)
        => services.AddMediatR(handlerAssemblyMarkerTypes, configuration: null);

    /// <summary>
    /// Registers handlers and mediator types from the assemblies that contain the specified types
    /// </summary>
    /// <param name="services"></param>
    /// <param name="handlerAssemblyMarkerTypes"></param>
    /// <param name="configuration">The action used to configure the options</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddMediatR(this IServiceCollection services, Action<MediatRServiceConfiguration>? configuration, params Type[] handlerAssemblyMarkerTypes)
        => services.AddMediatR(handlerAssemblyMarkerTypes, configuration);

    /// <summary>
    /// Registers handlers and mediator types from the assemblies that contain the specified types
    /// </summary>
    /// <param name="services"></param>
    /// <param name="handlerAssemblyMarkerTypes"></param>
    /// <param name="configuration">The action used to configure the options</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddMediatR(this IServiceCollection services, IEnumerable<Type> handlerAssemblyMarkerTypes, Action<MediatRServiceConfiguration>? configuration)
        => services.AddMediatR(handlerAssemblyMarkerTypes.Select(t => t.GetTypeInfo().Assembly), configuration);
}

/// <summary>
/// Extensions to scan for MediatR handlers and registers them.
/// Registers all open bound generic handlers by scanning the assembly
/// </summary>
public static class MediatorExtensions
{
    /// <summary>
    /// Registers all open bound generic handlers
    /// </summary>
    /// <param name="services">Microsoft DI IServicesCollection, in ConfigureServices</param>
    /// <param name="assembly">Assembly for which services to register</param>
    public static void RegisterGenericMediatRHandlers(this IServiceCollection services, Assembly assembly)
    {
        try
        {
            //Get all the types which inherit from IRequestHandler<,> i.e. all handlers
            foreach (Type implementationType in assembly.GetTypes()./*IsolateDebug().*/Where(x => x.IsClass && (x.BaseType == typeof(object)) && x.GetInterfaces().Where(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)).Any()))
            {
                //no need to register concrete handlers 
                if (!implementationType.IsGenericTypeDefinition) continue;

                //get all the constrains of the Handler<T>
                Dictionary<Type, Type[]> contrainedGenericTypes = implementationType.GetGenericTypeDefinition().GetGenericArguments().ToDictionary(x => x, x => x.GetGenericParameterConstraints());//.Select(x => x.GetGenericParameterConstraints()).FirstOrDefault();

                //contrainedGenericTypes.any([assemply type inherits constrained class] or [constrained interface is assignable from assebly type] or [contrained type is not generic(perhasps a class) and constrained type is assembly type]); where assembly type E(belongs to) Type in predicate method
                Dictionary<Type, Type[]> contrainedConcreteTypes = contrainedGenericTypes.Select(x => new { argument = x.Key, concreteTypes = assembly.GetTypes().Where(x => !x.IsInterface).Where(type => x.Value.Any(gType => type.IsSubclassOf(gType) || gType.IsAssignableFrom(type) || (!gType.IsGenericType && gType == type))).ToArray() }).ToDictionary(x => x.argument, x => x.concreteTypes);

                //get the(first) interface with which the handler is in contract i.e. IRequestHandler
                Type requestHandlerInterface = implementationType.GetInterfaces().Where(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)).FirstOrDefault();

                if (requestHandlerInterface != null)
                {
                    var allTypes = contrainedConcreteTypes.Values.Select(x => x.ToList()).ToList();
                    List<List<Type>> permutationsOfArguments = new List<List<Type>>();
                    GetPermutations(allTypes, new List<Type>(), permutationsOfArguments, allTypes.Count);
                    foreach (var concreteType in permutationsOfArguments)
                    {
                        //used to replace genric T(or U) with the concrete type in the arrangement
                        //create [{GenericType: ConcreteType}]
                        Dictionary<Type, Type> mappedConcreteTypes = concreteType.ToDictionary(arrangement => contrainedConcreteTypes.Where(category => category.Value.Contains(arrangement)).Select(dict => dict.Key).First(), arrangement => arrangement);

                        //get Query/Command generic class
                        Type genericQueryType = requestHandlerInterface.GetTypeInfo().GetGenericArguments().FirstOrDefault();
                        //create concrete Query<T>
                        Type concreteQueryType = MakeGenericType(genericQueryType, mappedConcreteTypes);

                        Type concreteImplementation = implementationType.MakeGenericType(concreteType.ToArray()); //Handler will use all the types in the arrangement, no need to resolve
                        
                        //get concrete return type
                        Type ConcreteReturnType = IRequestHandlerReturnType(requestHandlerInterface, mappedConcreteTypes);

                        //finally, concrete Handler type
                        Type concreteHandlerService = typeof(IRequestHandler<,>).MakeGenericType(concreteQueryType, ConcreteReturnType);

                        services.AddTransient(concreteHandlerService, concreteImplementation);
                    }
                }
            }
        }
        catch (Exception)
        {
            return;
        }
    }

    static void GetPermutations<T>(this List<List<T>> array, List<T> path, List<List<T>> retVal, int length)
    {
        foreach (var item in array.First())
        {
            if (array.Count == 1)
            {
                var clonedPath = path.ToList();
                path.Add(item);
                retVal.Add(path);
                if (path.Count == length)
                {
                    GetPermutations<T>(new List<List<T>>() { array.First().Skip(length - clonedPath.Count).ToList() }, clonedPath, retVal, length);
                    break;
                }
            }
            else
            {
                var clonedPath = path.ToList();
                clonedPath.Add(item);
                GetPermutations<T>(array.Skip(1).ToList(), clonedPath, retVal, length);
            }
        }
    }

    //TODO: recursively go through each `type = type.GetGenericArguments();` and then finally resolve the final type
    //in order to resolve types like: Dictionary<T, List<U>>
    static Type MakeGenericType(Type definition, Dictionary<Type, Type> parameter)
    {
        var definitionStack = new Stack<Type>();
        var type = definition;
        while (!type.IsGenericTypeDefinition && type.IsGenericType)
        {
            definitionStack.Push(type);
            type = type.GetGenericArguments()[0];
        }

        if (definitionStack.Count > 0)
        {
            Type typeInStack = definitionStack.Pop();
            type = typeInStack.GetGenericTypeDefinition().MakeGenericType(GetGenericTypeConcreteParams(typeInStack, parameter));
        }

        while (definitionStack.Count > 0)
            type = definitionStack.Pop().GetGenericTypeDefinition().MakeGenericType(type);
        return type;
    }

    static Type[] GetGenericTypeConcreteParams(Type definition, Dictionary<Type, Type> GenericConcretePairs)
    {
        return definition.GetGenericArguments().Select(generic => GenericConcretePairs[generic]).ToArray();
    }

    static Type IRequestHandlerReturnType(Type requestHandlerInterface, Dictionary<Type, Type> concreteType)
    {
        if (requestHandlerInterface == null)
            return typeof(Unit);
        var genericTypes = requestHandlerInterface.GetTypeInfo().GetGenericArguments();
        //it can either be 1 or 2, IRequestHandler<,>
        if (genericTypes.Count() == 1)
        {
            return typeof(Unit);
        }
        else if (genericTypes.Count() > 1) //if someday somebody decides to make IRequestHandler without any generic arguments
        {
            Type returnType = genericTypes[1];
            if (returnType.IsGenericType)
                return MakeGenericType(returnType, concreteType);
            else
                return returnType;
        }

        return typeof(Unit);
    }
}


