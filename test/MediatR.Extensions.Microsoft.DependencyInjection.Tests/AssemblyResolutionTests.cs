using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Extensions.Microsoft.DependencyInjection.Tests
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Shouldly;
    using Xunit;

    public class AssemblyResolutionTests
    {
        private readonly IServiceProvider _provider;

        public AssemblyResolutionTests()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton(new Logger());
            services.AddMediatR(typeof(Ping).GetTypeInfo().Assembly);
            _provider = services.BuildServiceProvider();
        }

        [Fact]
        public void ShouldResolveMediator()
        {
            _provider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void ShouldResolveRequestHandler()
        {
            _provider.GetService<IRequestHandler<Ping, Pong>>().ShouldNotBeNull();
        }

        [Fact]
        public void ShouldResolveNotificationHandlers()
        {
            _provider.GetServices<INotificationHandler<Pinged>>().Count().ShouldBe(3);
        }
    }
}