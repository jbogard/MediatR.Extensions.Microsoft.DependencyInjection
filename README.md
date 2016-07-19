# MediatR extensions for Microsoft.Extensions.DependencyInjection

Scans assemblies and adds handlers implementations to the container. To use, with an `IServiceCollection` instance:

```
services.AddMediatR(typeof(MyHander));
```

or with an assembly:

```
services.AddMediatR(typeof(Startup).Assembly);
```

Supports generic variance of handlers.
