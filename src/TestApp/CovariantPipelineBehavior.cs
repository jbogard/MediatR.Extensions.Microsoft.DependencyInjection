using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace TestApp
{
    public class CovariantPipelineBehavior<TResponse> : IPipelineBehavior<ICovariantPipelinable, TResponse>
    {
        private readonly TextWriter _writer;

        public CovariantPipelineBehavior(TextWriter writer)
        {
            _writer = writer;
        }

        public async Task<TResponse> Handle(ICovariantPipelinable request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            await _writer.WriteLineAsync("-- Got meowed");
            request.PipelineMessage = "Pipeline meow";
            return await next();
        }
    }
}