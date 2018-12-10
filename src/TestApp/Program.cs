using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Pipeline;

namespace TestApp
{

    public class Program
    {
        public static Task Main(string[] args)
        {
            var writer = new WrappingWriter(Console.Out);
            var mediator = BuildMediator(writer);
            return Runner.Run(mediator, writer, "ASP.NET Core DI");
        }

        private static IMediator BuildMediator(WrappingWriter writer)
        {
            var services = new ServiceCollection();

            services.AddScoped<ServiceFactory>(p => p.GetService);

            services.AddSingleton<TextWriter>(writer);

            //Pipeline
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestPreProcessorBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestPostProcessorBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(GenericPipelineBehavior<,>));
            services.AddScoped(typeof(IRequestPreProcessor<>), typeof(GenericRequestPreProcessor<>));
            services.AddScoped(typeof(IRequestPostProcessor<,>), typeof(GenericRequestPostProcessor<,>));

            //This causes a type load exception. https://github.com/jbogard/MediatR.Extensions.Microsoft.DependencyInjection/issues/12
            //services.AddScoped(typeof(IRequestPostProcessor<,>), typeof(ConstrainedRequestPostProcessor<,>));
            //services.AddScoped(typeof(INotificationHandler<>), typeof(ConstrainedPingedHandler<>));

            services.AddMediatR(typeof(Ping));

            var provider = services.BuildServiceProvider();

            return provider.GetRequiredService<IMediator>();
        }
    }
    
}
