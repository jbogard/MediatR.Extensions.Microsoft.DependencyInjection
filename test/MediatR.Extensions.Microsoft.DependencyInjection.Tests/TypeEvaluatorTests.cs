using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Extensions.Microsoft.DependencyInjection.Tests
{
    using System;
    using System.Linq;
    using System.Reflection;
    using MediatR.Extensions.Microsoft.DependencyInjection.Tests.Included;
    using Shouldly;
    using Xunit;

    public class TypeEvaluatorTests
    {
        private readonly IServiceProvider _provider;

        public TypeEvaluatorTests()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton(new Logger());
            services.AddMediatR(new[] { typeof(Ping).GetTypeInfo().Assembly }, t =>
            {
                return t.Namespace == "MediatR.Extensions.Microsoft.DependencyInjection.Tests.Included";
            });
            _provider = services.BuildServiceProvider();
        }

        [Fact]
        public void ShouldResolveMediator()
        {
            _provider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void ShouldOnlyResolveIncludedRequestHandlers()
        {
            _provider.GetService<IRequestHandler<Foo, Bar>>().ShouldNotBeNull();
            _provider.GetService<IRequestHandler<Ping, Pong>>().ShouldBeNull();
        }
    }
}
