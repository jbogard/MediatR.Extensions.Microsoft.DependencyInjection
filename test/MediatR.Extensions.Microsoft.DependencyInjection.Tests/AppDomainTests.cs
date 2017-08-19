using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Extensions.Microsoft.DependencyInjection.Tests
{
    using System;
    using System.Linq;
    using Shouldly;
    using Xunit;

    public class DependencyContextResolutionTests
    {
        private readonly IServiceProvider _provider;

        public DependencyContextResolutionTests()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton(new Logger());
            services.AddMediatR();
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
        public void ShouldResolveAsyncRequestHandler()
        {
            _provider.GetService<IAsyncRequestHandler<PingAsync, Pong>>().ShouldNotBeNull();
        }

        [Fact]
        public void ShouldResolveCancelableAsyncRequestHandler()
        {
            _provider.GetService<ICancellableAsyncRequestHandler<PingCancellableAsync, Pong>>().ShouldNotBeNull();
        }

        [Fact]
        public void ShouldResolveNotificationHandlers()
        {
            _provider.GetServices<INotificationHandler<Pinged>>().Count().ShouldBe(3);
        }

        [Fact]
        public void ShouldResolveAsyncNotificationHandlers()
        {
            _provider.GetServices<IAsyncNotificationHandler<PingedAsync>>().Count().ShouldBe(3);
        }
    }
}