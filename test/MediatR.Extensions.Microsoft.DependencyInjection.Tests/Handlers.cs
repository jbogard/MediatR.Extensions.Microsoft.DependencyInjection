using System;
using System.Runtime.CompilerServices;

namespace MediatR.Extensions.Microsoft.DependencyInjection.Tests
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class Ping : IRequest<Pong>
    {
        public string? Message { get; init; }
        public Action<Ping>? ThrowAction { get; init; }
    }

    public class DerivedPing : Ping
    {
    }

    public class Pong
    {
        public string? Message { get; init; }
    }

    public class Zing : IRequest<Zong>
    {
        public string? Message { get; init; }
    }

    public class Zong
    {
        public string? Message { get; init; }
    }

    public class Ding : IRequest
    {
        public string? Message { get; init; }
    }

    public class Pinged : INotification
    {

    }

    class InternalPing : IRequest { }

    public class StreamPing : IStreamRequest<Pong>
    {
        public string? Message { get; init; }
    }

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

            message.ThrowAction?.Invoke(message);

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

    public class PingStreamHandler : IStreamRequestHandler<StreamPing, Pong>
    {
        private readonly Logger _output;

        public PingStreamHandler(Logger output)
        {
            _output = output;
        }
        public async IAsyncEnumerable<Pong> Handle(StreamPing request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _output.Messages.Add("Handler");
            yield return await Task.Run(() => new Pong { Message = request.Message + " Pang" }, cancellationToken);
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
        public Task<object?> Send(object request, CancellationToken cancellationToken = new())
        {
            throw new System.NotImplementedException();
        }

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request,
            CancellationToken cancellationToken = new())
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = new())
        {
            throw new NotImplementedException();
        }

        public Task Publish(object notification, CancellationToken cancellationToken = new())
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

namespace MediatR.Extensions.Microsoft.DependencyInjection.Tests.Included
{
    using System.Threading;
    using System.Threading.Tasks;

    public class Foo : IRequest<Bar>
    {
        public string? Message { get; init; }
        public Action<Foo>? ThrowAction { get; init; }
    }

    public class Bar
    {
        public string? Message { get; init; }
    }

    public class FooHandler : IRequestHandler<Foo, Bar>
    {
        private readonly Logger _logger;

        public FooHandler(Logger logger)
        {
            _logger = logger;
        }
        public Task<Bar> Handle(Foo message, CancellationToken cancellationToken)
        {
            _logger.Messages.Add("Handler");

            message.ThrowAction?.Invoke(message);

            return Task.FromResult(new Bar { Message = message.Message + " Bar" });
        }
    }
}

namespace MediatR.Extensions.Microsoft.DependencyInjection.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;

    public interface ILayout
    {
        public string? Header { get; set; }
        public string? Body { get; set; }
        public string? Footer { get; set; }
    }
    public interface IInputPage
    {
        public string InputField { get; set; }
    }

    public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public bool Unauthorized { get; set; }

        public T? Value { get; set; }
        public string? Error { get; set; }

        public static Result<T> Success(T value) => new Result<T> { IsSuccess = true, Value = value };
        public static Result<T> Failure(string error, bool unauthorized = false) => new Result<T> { IsSuccess = false, Error = error, Unauthorized = unauthorized };
        public static Result<T> Failure(T value, string error, bool unauthorized = false) => new Result<T> { IsSuccess = false, Value = value, Error = error, Unauthorized = unauthorized };
    }

    public class SimpleGeneric
    {
        public class Query<T> : IRequest<Result<T>> where T : ILayout, new()
        {
            public int IDPage { get; set; }
        }

        public class Handler<T> : IRequestHandler<Query<T>, Result<T>> where T : ILayout, new()
        {
            public async Task<Result<T>> Handle(Query<T> request, CancellationToken cancellationToken)
            {
                var dto = new T() { Body = "lorem", Header = "Ipsum", Footer = "Odor" };
                if (dto is IInputPage inputDTO)
                    inputDTO.InputField = "Username";

                await Task.Run(() => null);

                return Result<T>.Success(dto);
            }
        }
    }

    public interface T_X
    {
        public int IDChild { get; set; }
    }

    public interface T_XDomains
    {
        public int IDParent { get; set; }
        public int IDChild { get; set; }
    }

    public class MaybeSimpleGeneric
    {
        public record Data<T, U> where T : T_X where U : T_XDomains
        {
            public U? T_Xdomains { get; set; }
            public T? T_X { get; set; }
        }

        public class Query<T, U> : IRequest<List<Data<T, U>>> where T : T_X where U : T_XDomains
        {
            public int IDPage { get; set; }
        }

        public class Handler<T, U> : IRequestHandler<Query<T, U>, List<Data<T, U>>> where T : T_X, new() where U : T_XDomains, new()
        {
            public async Task<List<Data<T, U>>> Handle(Query<T, U> request, CancellationToken cancellationToken)
            {
                await Task.Run(() => null);
                //join the tables with _application.Context with Child ID and return some data
                return new List<Data<T, U>>() { new Data<T, U>() { T_X = new() { IDChild = 1 }, T_Xdomains = new() { IDChild = 1, IDParent = 2 } } };
            }
        }
    }

    /// <summary>
    /// Will be inherited by return type DTOs
    /// </summary>
    public interface IOutside
    {
        public int Outside { get; set; }
    }

    public class ComplexGeneric
    {
        public record Data<T, U> where T : T_X where U : T_XDomains
        {
            public U? T_Xdomains { get; set; }
            public T? T_X { get; set; }
        }

        public class Query<T, U, R> : IRequest<Result<List<R>>> where T : class, T_X, new() where U : class, T_XDomains, new() where R : class, IOutside, new()
        {
            public int IDPage { get; set; }
            public Func<Data<T, U>, R>? SelectQuery { get; set; }
        }

        public class Handler<T, U, R> : IRequestHandler<Query<T, U, R>, Result<List<R>>> where T : class, T_X, new() where U : class, T_XDomains, new() where R : class, IOutside, new()
        {
            public async Task<Result<List<R>>> Handle(Query<T, U, R> request, CancellationToken cancellationToken)
            {
                await Task.Run(() => null);
                //join the tables with _application.Context with Child ID and return some data
                var dataAfterJoiningSaidTables = new List<Data<T, U>>() { new Data<T, U>() { T_X = new() { IDChild = 1 }, T_Xdomains = new() { IDChild = 1, IDParent = 2 } } };
                var data = request.SelectQuery?.Invoke(dataAfterJoiningSaidTables.First());

                if (data != null)
                    return Result<List<R>>.Success(new List<R>() { data }); //.ToListAsync()
                else
                    return Result<List<R>>.Failure("An error orccured!");
            }
        }
    }
}