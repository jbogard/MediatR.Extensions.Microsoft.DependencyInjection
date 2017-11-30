using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TestApp
{

    public class Program
    {
        public static void Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddMediatR();
//            var provider = services.BuildServiceProvider();

            foreach (var service in services)
            {
                Console.WriteLine(service.ServiceType + " - " + service.ImplementationType);
            }
            Console.ReadKey();
        }
    }


    public class Ping : IRequest<Pong>
    {
        public string Message { get; set; }
    }

    public class PingAsync : IRequest<Pong>
    {
        public string Message { get; set; }
    }

    public class PingCancellableAsync : IRequest<Pong>
    {
        public string Message { get; set; }
    }

    public class Pong
    {
        public string Message { get; set; }
    }

    public class Pinged : INotification
    {

    }

    public class PingedAsync : INotification
    {

    }

    public class GenericHandler : INotificationHandler<INotification>
    {
        public Task Handle(INotification notification, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }

    public class PingedHandler : INotificationHandler<Pinged>
    {
        public Task Handle(Pinged notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class PingedAlsoHandler : INotificationHandler<Pinged>
    {
        public Task Handle(Pinged notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class PingHandler : IRequestHandler<Ping, Pong>
    {
        public Task<Pong> Handle(Ping message, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Pong { Message = message.Message + " Pong" });
        }
    }
}
