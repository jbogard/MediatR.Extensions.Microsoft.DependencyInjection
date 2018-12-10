using MediatR;

namespace TestApp
{
    public class Ping : IRequest<Pong>
    {
        public string Message { get; set; }
    }
}