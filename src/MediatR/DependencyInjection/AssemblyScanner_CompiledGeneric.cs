﻿using System;
using System.Collections.Generic;
using System.Linq;
using MediatR.Abstraction.Behaviors;
using MediatR.Abstraction.ExceptionHandler;
using MediatR.Abstraction.Handlers;
using MediatR.Abstraction.Pipeline;

namespace MediatR.DependencyInjection;

internal partial struct AssemblyScanner<TRegistrar>
{
    private void AddExceptionHandingInterfaces(List<(Type, bool)> implementingInterfaces, Type typeVariant)
    {
        AddNoneGenericInterfaceImplementations(typeVariant, typeof(IRequestExceptionAction<,>), implementingInterfaces, false);
        AddNoneGenericInterfaceImplementations(typeVariant, typeof(IRequestExceptionHandler<,>), implementingInterfaces, true);
        AddNoneGenericInterfaceImplementations(typeVariant, typeof(IRequestResponseExceptionAction<,,>), implementingInterfaces, false);
        AddNoneGenericInterfaceImplementations(typeVariant, typeof(IRequestResponseExceptionHandler<,,>), implementingInterfaces, true);
    }

    private void AddProcessorInterfaces(List<(Type, bool)> implementingInterfaces, Type typeVariant)
    {
        AddNoneGenericInterfaceImplementations(typeVariant, typeof(IRequestPreProcessor<>), implementingInterfaces, false);
        AddNoneGenericInterfaceImplementations(typeVariant, typeof(IRequestPreProcessor<,>), implementingInterfaces, false);
        AddNoneGenericInterfaceImplementations(typeVariant, typeof(IRequestPostProcessor<>), implementingInterfaces, false);
        AddNoneGenericInterfaceImplementations(typeVariant, typeof(IRequestPostProcessor<,>), implementingInterfaces, false);
    }

    private void AddHandlerInterfaces(List<(Type, bool)> implementingInterfaces, Type typeVariant)
    {
        AddNoneGenericInterfaceImplementations(typeVariant, typeof(INotificationHandler<>), implementingInterfaces, false);
        AddNoneGenericInterfaceImplementations(typeVariant, typeof(IRequestHandler<>), implementingInterfaces, true);
        AddNoneGenericInterfaceImplementations(typeVariant, typeof(IRequestHandler<,>), implementingInterfaces, true);
        AddNoneGenericInterfaceImplementations(typeVariant, typeof(IStreamRequestHandler<,>), implementingInterfaces, true);
    }

    private void AddPipelineInterfaces(List<(Type, bool)> implementingInterfaces, Type typeVariant)
    {
        AddNoneGenericPipelineInterfaceImplementations(typeVariant, typeof(IPipelineBehavior<>), implementingInterfaces, false);
        AddNoneGenericPipelineInterfaceImplementations(typeVariant, typeof(IPipelineBehavior<,>), implementingInterfaces, false);
        AddNoneGenericPipelineInterfaceImplementations(typeVariant, typeof(IStreamPipelineBehavior<,>), implementingInterfaces, false);
    }

    private static void AddNoneGenericInterfaceImplementations(
        Type type,
        Type openGenericInterface,
        List<(Type, bool)> implementingInterfaces,
        bool mustBeSingleRegistration)
        => implementingInterfaces.AddRange(
            type.GetInterfaces()
                .Where(t =>
                    !t.ContainsGenericParameters &&
                    t.IsGenericType &&
                    t.GetGenericTypeDefinition() == openGenericInterface)
                .Select(t => (t, mustBeSingleRegistration)));

    private void AddNoneGenericPipelineInterfaceImplementations(
        Type typeVariant,
        Type openGenericPipelines,
        List<(Type, bool)> implementingInterfaces,
        bool mustBeSingleRegistration)
    {
        var config = _configuration;
        var typeComparer = _typeComparer;

        implementingInterfaces.AddRange(
            typeVariant.GetInterfaces()
                .Where(t =>
                    !t.ContainsGenericParameters &&
                    t.IsGenericType &&
                    t.GetGenericTypeDefinition() == openGenericPipelines &&
                    !IsPreRegisteredBehavior(typeVariant, t))
                .Select(t => (t, mustBeSingleRegistration)));

        bool IsPreRegisteredBehavior(Type type, Type interfaceType)
        {
            if (config.BehaviorsToRegister.TryGetValue(type, out var interfaces))
            {
                return Array.BinarySearch(interfaces, interfaceType, typeComparer) >= 0;
            }

            return false;
        }
    }
}