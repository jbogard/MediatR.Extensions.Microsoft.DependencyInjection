using System;
using System.IO;
using System.Threading;
using MediatR;

namespace TestApp
{
    using System.Threading.Tasks;

    public class PingHandler : IRequestHandler<Ping, Pong>
    {
        private readonly TextWriter _writer;

        public PingHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public async Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
        {
            await _writer.WriteLineAsync($"--- Handled Ping: {request.Message}");

            if (request.Throw)
            {
                throw new ApplicationException("Requested to throw");
            }

            return new Pong { Message = request.Message + " Pong" };
        }
    }
}