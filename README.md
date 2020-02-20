# MediatR extensions for Microsoft.Extensions.DependencyInjection

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
- `IRequestPreProcessor<>` concrete implementations as transient
- `IRequestHandler<>` concrete implementations as transient
- `IRequestPostProcessor<,>` concrete implementations as transient
- `IRequestExceptionHandler<,,>` concrete implementations as transient

This also registers open generic implementations for:

- `INotificationHandler<>`
- `IRequestPreProcessor<>`
- `IRequestHandler<>`
- `IRequestPostProcessor<,>`
- `IRequestExceptionHandler<,,>`

Keep in mind that the built-in container does not support constrained open generics. If you want this behavior, you will need to add any one of the conforming containers.

To customize registration, such as lifecycle or the registration type:

```c#
services.AddMediatR(cfg => cfg.Using<MyCustomMediator>().AsSingleton(), typeof(Startup));
```

To register behaviors, register them individually before or after calling `AddMediatR`.