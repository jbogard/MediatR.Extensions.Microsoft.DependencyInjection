# MediatR extensions for Microsoft.Extensions.DependencyInjection

Scans assemblies and adds handlers, preprocessors, and postprocessors implementations to the container. To use, with an `IServiceCollection` instance:

```
services.AddMediatR(typeof(MyHandler));
```

or with an assembly:

```
services.AddMediatR(typeof(Startup).GetTypeInfo().Assembly);
```

Supports generic variance of handlers.

To customize registration, such as lifecycle or the registration type:

```c#
services.AddMediatR(cfg => cfg.Using<MyCustomMediator>().AsSingleton(), typeof(Startup));
```

To register behaviors, pre- or post-processors, register them individually before or after calling `AddMediatR`.