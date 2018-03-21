namespace MediatR.Extensions.Microsoft.DependencyInjection.Tests
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class Ping : IRequest<Pong>
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
        public Task Handle(Ding message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
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
        public Task Handle(InternalPing request, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}