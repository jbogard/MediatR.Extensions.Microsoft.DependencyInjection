using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Extensions.Microsoft.DependencyInjection.Tests
{
    using System;
    using System.Linq;
    using Shouldly;
    using Xunit;

    public class CustomMediatorTests
    {
        private readonly IServiceProvider _provider;

        public CustomMediatorTests()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton(new Logger());
            services.AddMediatR(cfg => cfg.Using<MyCustomMediator>(), typeof(CustomMediatorTests));
            _provider = services.BuildServiceProvider();
        }

        [Fact]
        public void ShouldResolveMediator()
        {
            _provider.GetService<IMediator>().ShouldNotBeNull();
            _provider.GetService<IMediator>().GetType().ShouldBe(typeof(MyCustomMediator));
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