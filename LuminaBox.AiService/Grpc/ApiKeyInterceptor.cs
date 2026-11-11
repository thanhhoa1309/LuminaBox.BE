using System.Security.Cryptography;
using System.Text;
using Grpc.Core;
using Grpc.Core.Interceptors;
using LuminaBox.AiService.Options;
using Microsoft.Extensions.Options;

namespace LuminaBox.AiService.Grpc;

/// <summary>Rejects calls whose <c>x-api-key</c> metadata does not match <see cref="AiServiceOptions.ApiKey"/>.</summary>
public sealed class ApiKeyInterceptor : Interceptor
{
    public const string HeaderName = "x-api-key";

    private readonly byte[]? _expected;

    public ApiKeyInterceptor(IOptions<AiServiceOptions> options)
    {
        var key = options.Value.ApiKey;
        _expected = string.IsNullOrEmpty(key) ? null : Encoding.UTF8.GetBytes(key);
    }

    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        Check(context);
        return continuation(request, context);
    }

    public override Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        Check(context);
        return continuation(request, responseStream, context);
    }

    private void Check(ServerCallContext context)
    {
        if (_expected is null)
            return;

        var provided = context.RequestHeaders.GetValue(HeaderName);
        if (provided is null || !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(provided), _expected))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid API key."));
    }
}
