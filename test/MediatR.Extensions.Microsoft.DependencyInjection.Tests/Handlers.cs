namespace MediatR.Extensions.Microsoft.DependencyInjection.Tests
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class Ping : IRequest<Pong>
    {
        public string Message { get; set; }
    }

    public class DerivedPing : Ping
    {
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

    public class Ding : IRequest
    {
        public string Message { get; set; }
    }

    public class Pinged : INotification
    {

    }

    class InternalPing : IRequest { }

    public class GenericHandler : INotificationHandler<INotification>
    {
        public Task Handle(INotification notification, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }

    public class DingAsyncHandler : IRequestHandler<Ding>
    {
        public Task<Unit> Handle(Ding message, CancellationToken cancellationToken) => Unit.Task;
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
        public Task<Pong> Handle(Ping message, CancellationToken cancellationToken)
        {
            _logger.Messages.Add("Handler");
            return Task.FromResult(new Pong { Message = message.Message + " Pong" });
        }
    }

    public class DerivedPingHandler : IRequestHandler<DerivedPing, Pong>
    {
        private readonly Logger _logger;

        public DerivedPingHandler(Logger logger)
        {
            _logger = logger;
        }
        public Task<Pong> Handle(DerivedPing message, CancellationToken cancellationToken)
        {
            _logger.Messages.Add("Handler");
            return Task.FromResult(new Pong { Message = $"Derived{message.Message} Pong" });
        }
    }

    public class ZingHandler : IRequestHandler<Zing, Zong>
    {
        private readonly Logger _output;

        public ZingHandler(Logger output)
        {
            _output = output;
        }
        public Task<Zong> Handle(Zing message, CancellationToken cancellationToken)
        {
            _output.Messages.Add("Handler");
            return Task.FromResult(new Zong { Message = message.Message + " Zong" });
        }
    }

    public class DuplicateTest : IRequest<string> { }
    public class DuplicateHandler1 : IRequestHandler<DuplicateTest, string>
    {
        public Task<string> Handle(DuplicateTest message, CancellationToken cancellationToken)
        {
            return Task.FromResult(nameof(DuplicateHandler1));
        }
    }

    public class DuplicateHandler2 : IRequestHandler<DuplicateTest, string>
    {
        public Task<string> Handle(DuplicateTest message, CancellationToken cancellationToken)
        {
            return Task.FromResult(nameof(DuplicateHandler2));
        }
    }

    class InternalPingHandler : IRequestHandler<InternalPing>
    {
        public Task<Unit> Handle(InternalPing request, CancellationToken cancellationToken) => Unit.Task;
    }

    class MyCustomMediator : IMediator
    {
        public Task Publish(object notification, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
        {
            throw new System.NotImplementedException();
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }

}
