namespace MediatR.Extensions.Microsoft.DependencyInjection.Tests
{
    using System.Threading;
    using System.Threading.Tasks;

    public class Ping : IRequest<Pong>
    {
        public string Message { get; set; }
    }

    public class PingAsync : IAsyncRequest<Pong>
    {
        public string Message { get; set; }
    }

    public class PingCancellableAsync : ICancellableAsyncRequest<Pong>
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

    public class PingedAsync : IAsyncNotification
    {

    }

    public class GenericAsyncHandler : IAsyncNotificationHandler<IAsyncNotification>
    {
        public Task Handle(IAsyncNotification notification)
        {
            return Task.FromResult(0);
        }
    }

    public class GenericHandler : INotificationHandler<INotification>
    {
        public void Handle(INotification notification)
        {
        }
    }

    public class PingAsyncHandler : IAsyncRequestHandler<PingAsync, Pong>
    {
        public Task<Pong> Handle(PingAsync message)
        {
            return Task.FromResult(new Pong());
        }
    }

    public class PingCancellableAsyncHandler : ICancellableAsyncRequestHandler<PingCancellableAsync, Pong>
    {
        public Task<Pong> Handle(PingCancellableAsync message, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Pong());
        }
    }

    public class PingedAsyncHandler : IAsyncNotificationHandler<PingedAsync>
    {
        public Task Handle(PingedAsync notification)
        {
            return Task.FromResult(0);
        }
    }

    public class PingedAlsoAsyncHandler : IAsyncNotificationHandler<PingedAsync>
    {
        public Task Handle(PingedAsync notification)
        {
            return Task.FromResult(0);
        }
    }

    public class PingedHandler : INotificationHandler<Pinged>
    {
        public void Handle(Pinged notification)
        {
        }
    }

    public class PingedAlsoHandler : INotificationHandler<Pinged>
    {
        public void Handle(Pinged notification)
        {
        }
    }

    public class PingHandler : IRequestHandler<Ping, Pong>
    {
        public Pong Handle(Ping message)
        {
            return new Pong { Message = message.Message + " Pong" };
        }
    }
}