using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Extensions.Microsoft.DependencyInjection.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Shouldly;
    using Xunit;

    public class PipelineTests
    {
        public class OuterBehavior : IPipelineBehavior<Ping, Pong>
        {
            private readonly Logger _output;

            public OuterBehavior(Logger output)
            {
                _output = output;
            }

            public async Task<Pong> Handle(Ping request, RequestHandlerDelegate<Pong> next)
            {
                _output.Messages.Add("Outer before");
                var response = await next();
                _output.Messages.Add("Outer after");

                return response;
            }
        }

        public class InnerBehavior : IPipelineBehavior<Ping, Pong>
        {
            private readonly Logger _output;

            public InnerBehavior(Logger output)
            {
                _output = output;
            }

            public async Task<Pong> Handle(Ping request, RequestHandlerDelegate<Pong> next)
            {
                _output.Messages.Add("Inner before");
                var response = await next();
                _output.Messages.Add("Inner after");

                return response;
            }
        }

        public class InnerBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        {
            private readonly Logger _output;

            public InnerBehavior(Logger output)
            {
                _output = output;
            }

            public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next)
            {
                _output.Messages.Add("Inner generic before");
                var response = await next();
                _output.Messages.Add("Inner generic after");

                return response;
            }
        }

        public class OuterBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        {
            private readonly Logger _output;

            public OuterBehavior(Logger output)
            {
                _output = output;
            }

            public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next)
            {
                _output.Messages.Add("Outer generic before");
                var response = await next();
                _output.Messages.Add("Outer generic after");

                return response;
            }
        }

        public class ConstrainedBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
            where TRequest : Ping
            where TResponse : Pong
        {
            private readonly Logger _output;

            public ConstrainedBehavior(Logger output)
            {
                _output = output;
            }

            public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next)
            {
                _output.Messages.Add("Constrained before");
                var response = await next();
                _output.Messages.Add("Constrained after");

                return response;
            }
        }

        [Fact]
        public async Task Should_wrap_with_behavior()
        {
            var output = new Logger();
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton(output);
            services.AddTransient<IPipelineBehavior<Ping, Pong>, OuterBehavior>();
            services.AddTransient<IPipelineBehavior<Ping, Pong>, InnerBehavior>();
            services.AddMediatR(typeof(Ping).GetTypeInfo().Assembly);
            var provider = services.BuildServiceProvider();

            var mediator = provider.GetService<IMediator>();

            var response = await mediator.Send(new Ping { Message = "Ping" });

            response.Message.ShouldBe("Ping Pong");

            output.Messages.ShouldBe(new[]
            {
                "Outer before",
                "Inner before",
                "Handler",
                "Inner after",
                "Outer after"
            });
        }


        [Fact]
        public async Task Should_wrap_generics_with_behavior()
        {
            var output = new Logger();
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton(output);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(OuterBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(InnerBehavior<,>));
            services.AddMediatR(typeof(Ping).GetTypeInfo().Assembly);
            var provider = services.BuildServiceProvider();

            var mediator = provider.GetService<IMediator>();

            var response = await mediator.Send(new Ping { Message = "Ping" });

            response.Message.ShouldBe("Ping Pong");

            output.Messages.ShouldBe(new[]
            {
                "Outer generic before",
                "Inner generic before",
                "Handler",
                "Inner generic after",
                "Outer generic after",
            });
        }

        [Fact(Skip = "MS DI does not support constrained generics yet, see https://github.com/aspnet/DependencyInjection/issues/471")]
        public async Task Should_handle_constrained_generics()
        {
            var output = new Logger();
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton(output);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(OuterBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(InnerBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ConstrainedBehavior<,>));
            services.AddMediatR(typeof(Ping).GetTypeInfo().Assembly);
            var provider = services.BuildServiceProvider();

            var mediator = provider.GetService<IMediator>();

            var response = await mediator.Send(new Ping { Message = "Ping" });

            response.Message.ShouldBe("Ping Pong");

            output.Messages.ShouldBe(new[]
            {
                "Outer generic before",
                "Inner generic before",
                "Constrained before",
                "Handler",
                "Constrained after",
                "Inner generic after",
                "Outer generic after",
            });

            output.Messages.Clear();

            var zingResponse = await mediator.Send(new Zing { Message = "Zing" });

            zingResponse.Message.ShouldBe("Zing Zong");

            output.Messages.ShouldBe(new[]
            {
                "Outer generic before",
                "Inner generic before",
                "Handler",
                "Inner generic after",
                "Outer generic after",
            });
        }

    }
}