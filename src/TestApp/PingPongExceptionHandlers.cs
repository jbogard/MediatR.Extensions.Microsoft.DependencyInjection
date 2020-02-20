using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Pipeline;

namespace TestApp
{
    public class PingPongExceptionHandlerForType : IRequestExceptionHandler<Ping, Pong, ApplicationException>
    {
        public Task Handle(Ping request, ApplicationException exception, RequestExceptionHandlerState<Pong> state, CancellationToken cancellationToken)
        {
            state.SetHandled(new Pong { Message = exception.Message + " Handled by Type" });

            return Task.CompletedTask;
        }
    }

    public class PingPongExceptionActionForType1 : IRequestExceptionAction<Ping, ApplicationException>
    {
        private readonly TextWriter _output;

        public PingPongExceptionActionForType1(TextWriter output) => _output = output;

        public Task Execute(Ping request, ApplicationException exception, CancellationToken cancellationToken)
            => _output.WriteLineAsync("Logging exception 1");
    }

    public class PingPongExceptionActionForType2 : IRequestExceptionAction<Ping, ApplicationException>
    {
        private readonly TextWriter _output;

        public PingPongExceptionActionForType2(TextWriter output) => _output = output;

        public Task Execute(Ping request, ApplicationException exception, CancellationToken cancellationToken)
            => _output.WriteLineAsync("Logging exception 2");
    }


}