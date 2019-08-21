using MediatR;

namespace TestApp
{
	public class Meow : IRequest, ICovariantPipelinable
	{
		public string PipelineMessage { get; set; }
		public string Message { get; set; }
	}
}