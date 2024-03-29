using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediatR.Pipeline;

namespace TestApp;

public class ConstrainedRequestPostProcessor<TRequest, TResponse>
    : IRequestPostProcessor<TRequest, TResponse>
    where TRequest : Ping, IRequest<TResponse>
{
    private readonly TextWriter _writer;

    public ConstrainedRequestPostProcessor(TextWriter writer)
    {
        _writer = writer;
    }

    public Task Process(TRequest request, TResponse response, CancellationToken token)
    {
        return _writer.WriteLineAsync("- All Done with Ping");
    }
}