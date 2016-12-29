namespace MediatR.Extensions.Microsoft.DependencyInjection.Tests
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

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

    public class Zing : IRequest<Zong>
    {
        public string Message { get; set; }
    }

    public class Zong
    {
        public string Message { get; set; }
    }


    public class Pinged : INotification
    {

    }

    public class PingedAsync : INotification
    {

    }

    public class GenericAsyncHandler : IAsyncNotificationHandler<INotification>
    {
        public Task Handle(INotification notification)
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

    public class Logger
    {
        public IList<string> Messages { get; } = new List<string>();
    }

    public class PingHandler : IRequestHandler<Ping, Pong>
    {
        private readonly Logger _logger;

        public PingHandler(Logger logger)
        {
            _logger = logger;
        }
        public Pong Handle(Ping message)
        {
            _logger.Messages.Add("Handler");
            return new Pong { Message = message.Message + " Pong" };
        }
    }

    public class ZingHandler : IAsyncRequestHandler<Zing, Zong>
    {
        private readonly Logger _output;

        public ZingHandler(Logger output)
        {
            _output = output;
        }
        public Task<Zong> Handle(Zing message)
        {
            _output.Messages.Add("Handler");
            return Task.FromResult(new Zong { Message = message.Message + " Zong" });
        }
    }

}