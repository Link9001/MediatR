using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Processors;

namespace MediatR.Examples;

public class ConstrainedRequestPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
    where TRequest : Ping, IRequest<TResponse>
{
    private readonly TextWriter _writer;

    public ConstrainedRequestPostProcessor(TextWriter writer)
    {
        _writer = writer;
    }

    public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
    {
        return _writer.WriteLineAsync("- All Done with Ping");
    }
}