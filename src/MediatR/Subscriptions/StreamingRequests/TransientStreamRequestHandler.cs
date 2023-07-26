﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using MediatR.Abstraction.Behaviors;
using MediatR.Abstraction.Handlers;

namespace MediatR.Subscriptions.StreamingRequests;

internal sealed class TransientStreamRequestHandler<TRequest, TResponse> : StreamRequestHandler
    where TRequest : IStreamRequest<TResponse>
{
    public override IAsyncEnumerable<TMethodResponse> Handle<TMethodResponse>(IStreamRequest<TMethodResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var behaviors = serviceProvider.GetServices<IStreamPipelineBehavior<TRequest, TResponse>>();
        StreamHandlerNext<TRequest, TResponse> handler = GetHandler(serviceProvider).Handle;
        for (var i = behaviors.Length - 1; i >= 0; i--)
        {
            var next = handler;
            var behavior = behaviors[i];
            handler = (behaviorRequest, token) => behavior.Handle(behaviorRequest, next, token);
        }

        return (IAsyncEnumerable<TMethodResponse>) handler((TRequest) request, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IStreamRequestHandler<TRequest, TResponse> GetHandler(IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<IStreamRequestHandler<TRequest, TResponse>>();
}