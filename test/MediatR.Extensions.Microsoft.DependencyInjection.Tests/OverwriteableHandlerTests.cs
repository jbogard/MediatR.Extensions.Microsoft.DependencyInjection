using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shouldly;
using Xunit;

namespace MediatR.Extensions.Microsoft.DependencyInjection.Tests
{
    public class OverwriteableHandlerTests
    {
        [Fact]
        public async Task ShouldReturnOverwritablePong()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton(new Logger());
            services.AddMediatR(typeof(OverwriteablePingHandler));

            // Because the OverwriteablePingHandler and the OverwrittenPingHandler are in the same assembly, 
            // the OverwriteablePingHandler will be automatically be overwritten by the OverwrittenPingHandler.
            // So in this test scenario i need to replace back the OverwrittenPingHandler with the OverwriteablePingHandler as work around.
            services.Replace(new ServiceDescriptor(
                typeof(IRequestHandler<OverwriteablePing, IPong>),
                typeof(OverwriteablePingHandler),
                ServiceLifetime.Transient));

            using (ServiceProvider provider = services.BuildServiceProvider())
            {
                IMediator mediator = provider.GetRequiredService<IMediator>();

                Pong pong = (Pong)await mediator.Send(new OverwriteablePing() { Message = "Ping" });
                pong.Message.ShouldBe("Overwriteable Ping Pong");
            }
        }

        [Fact]
        public async Task ShouldReturnOverwrittenPong()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton(new Logger());
            services.AddMediatR(typeof(OverwriteablePingHandler));
            services.AddMediatR(typeof(OverwrittenPingHandler));

            using (ServiceProvider provider = services.BuildServiceProvider())
            {
                IMediator mediator = provider.GetRequiredService<IMediator>();

                OverwrittenPong pong = (OverwrittenPong)await mediator.Send(new OverwriteablePing() { Message = "Ping" });
                pong.Message.ShouldBe("Overwritten Ping Pong");
                pong.IsOverwritten.ShouldBe(true);
            }
        }
    }
}
