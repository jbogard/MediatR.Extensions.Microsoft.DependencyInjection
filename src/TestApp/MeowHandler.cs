using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace TestApp
{
	public class MeowHandler : IRequestHandler<Meow>
	{
		private readonly TextWriter _textWriter;

		public MeowHandler(TextWriter textWriter)
		{
			_textWriter = textWriter;
		}

		public async Task<Unit> Handle(Meow request, CancellationToken cancellationToken)
		{
			await _textWriter.WriteLineAsync($"--- {request.Message} and {request.PipelineMessage}");
			return Unit.Value;
		}
	}
}