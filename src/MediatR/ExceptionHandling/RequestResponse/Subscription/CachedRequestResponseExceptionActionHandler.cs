﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.ExceptionHandler;

namespace MediatR.ExceptionHandling.RequestResponse.Subscription;

internal sealed class CachedRequestResponseExceptionActionHandler<TRequest, TResponse, TException> : RequestResponseExceptionActionHandler
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
    where TException : Exception
{
    private IRequestResponseExceptionAction<TRequest, TResponse, TException>[]? _cachedHandler;

    public override Task Handle<TMethodRequest, TMethodResponse>(TMethodRequest request, Exception exception, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        Debug.Assert(typeof(TRequest).IsAssignableFrom(typeof(TMethodRequest)), "request type must be an inherited type of method request type.");
        Debug.Assert(typeof(TResponse) == typeof(TMethodResponse), "Response type and method response type must be the same type.");

        var handlers = (IRequestResponseExceptionAction<TMethodRequest, TMethodResponse, TException>[])GetHandler(serviceProvider);

        var tasks = new Task[handlers.Length];
        for (var i = 0; i < handlers.Length; i++)
        {
            tasks[i] = handlers[i].Execute(request, (TException) exception, cancellationToken);
        }

        return Task.WhenAll(tasks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IRequestResponseExceptionAction<TRequest, TResponse, TException>[] GetHandler(IServiceProvider serviceProvider) =>
        _cachedHandler ??= serviceProvider.GetServices<IRequestResponseExceptionAction<TRequest, TResponse, TException>>();
}