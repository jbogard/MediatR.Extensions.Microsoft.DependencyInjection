# MediatR extensions for Microsoft.Extensions.DependencyInjection

![CI](https://github.com/jbogard/MediatR.Extensions.Microsoft.DependencyInjection/workflows/CI/badge.svg)
[![NuGet](https://img.shields.io/nuget/dt/mediatr.extensions.microsoft.dependencyinjection.svg)](https://www.nuget.org/packages/mediatr.extensions.microsoft.dependencyinjection) 
[![NuGet](https://img.shields.io/nuget/vpre/mediatr.extensions.microsoft.dependencyinjection.svg)](https://www.nuget.org/packages/mediatr.extensions.microsoft.dependencyinjection)
[![MyGet (dev)](https://img.shields.io/myget/mediatr-ci/v/mediatr.extensions.microsoft.dependencyinjection.svg)](https://myget.org/gallery/mediatr-ci)

Scans assemblies and adds handlers, preprocessors, and postprocessors implementations to the container. To use, with an `IServiceCollection` instance:

```
services.AddMediatR(typeof(MyHandler));
```

or with an assembly:

```
services.AddMediatR(typeof(Startup).GetTypeInfo().Assembly);
```

This registers:

- `IMediator` as transient
- `IRequestHandler<>` concrete implementations as transient
- `INotificationHandler<>` concrete implementations as transient
- `IStreamRequestHandler<>` concrete implementations as transient
- `IRequestPreProcessor<>` concrete implementations as transient
- `IRequestPostProcessor<,>` concrete implementations as transient
- `IRequestExceptionHandler<,,>` concrete implementations as transient
- `IRequestExceptionAction<,>)` concrete implementations as transient

This also registers open generic implementations for:

- `INotificationHandler<>`
- `IRequestPreProcessor<>`
- `IRequestPostProcessor<,>`
- `IRequestExceptionHandler<,,>`
- `IRequestExceptionAction<,>`

Keep in mind that the built-in container does not support constrained open generics. If you want this behavior, you will need to add any one of the conforming containers.

To customize registration, such as lifecycle or the registration type:

```c#
services.AddMediatR(cfg => cfg.Using<MyCustomMediator>().AsSingleton(), typeof(Startup));
```

To register behaviors, register them individually before or after calling `AddMediatR`.

### Open generics

If you have an open generic not listed above, you'll need to register it explicitly. For example, if you have an open generic request handler, register the open generic types explicitly:

```csharp
services.AddTransient(typeof(IRequestHandler<,>), typeof(GenericHandlerBase<,>));
```

This won't work with generic constraints, so you're better off creating an abstract base class and concrete closed generic classes that fill in the right types.
Or you can add **Autofac** and add generic handlers.
```C#
public static ContainerBuilder AddGenericHandlers(this ContainerBuilder builder)
{
    builder.RegisterGeneric(typeof(GenericHandlerBase<,>.Handler)).AsImplementedInterfaces();
    //or
    builder.RegisterGeneric(typeof(AddNoteCommandHandler<>)).AsImplementedInterfaces();    
    return builder;
}
```
```
