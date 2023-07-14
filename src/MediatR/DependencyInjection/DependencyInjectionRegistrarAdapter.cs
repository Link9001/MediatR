﻿using System;
using System.Collections.Generic;

namespace MediatR.DependencyInjection;

/// <summary>
/// Declares the DiC Registrar Adapter
/// </summary>
/// <typeparam name="TRegistrar">The Registrar type.</typeparam>
public sealed class DependencyInjectionRegistrarAdapter<TRegistrar>
{
    /// <summary>
    /// Get the registrar to setup MediatR.
    /// </summary>
    public TRegistrar Registrar { get; }
    
    /// <summary>
    /// Defines with the <typeparamref name="TRegistrar"/> how to register a type to it self as a singleton and makes sure it is only registered once.
    /// </summary>
    public Action<TRegistrar, Type> RegisterSelfSingletonOnlyOnce { get; }

    /// <summary>
    /// Defines with the <typeparamref name="TRegistrar"/> how to register a service type to the implementation type as a singleton and makes sure it is only registered once.
    /// </summary>
    public Action<TRegistrar, Type, Type> RegisterSingletonOnlyOnce { get; }
    
    /// <summary>
    /// Defines with the <typeparamref name="TRegistrar"/> how to register a service type with its instance already created.
    /// </summary>
    public Action<TRegistrar, Type, object> RegisterInstance { get; }
    
    /// <summary>
    /// Defines with the <typeparamref name="TRegistrar"/> how to register a service type with its implementing type already registered but should be forwarded to this service type.
    /// </summary>
    public Action<TRegistrar, Type, Type> RegisterMapping { get; }
    
    /// <summary>
    /// Defines with the <typeparamref name="TRegistrar"/> how to register a service type with its implementing type already registered but should be forwarded to this service type and makes sure that its mapping is only registered once.
    /// </summary>
    public Action<TRegistrar, Type, Type> RegisterMappingOnlyOnce { get; }

    /// <summary>
    /// Defines with the <typeparamref name="TRegistrar"/> how to register a service type to the implementation type as a transient.
    /// </summary>
    public Action<TRegistrar, Type, Type> RegisterTransient { get; }

    /// <summary>
    /// Defines with the <typeparamref name="TRegistrar"/> how to register a service type to the implementation type as a transient and makes sure that it is only registered once.
    /// </summary>
    public Action<TRegistrar, Type, Type> RegisterTransientOnlyOnce { get; }

    /// <summary>
    /// Creates an adapter to register all MediatR Services.
    /// </summary>
    /// <param name="registrar">The registrar to setup MediatR.</param>
    /// <param name="registerSelfSingletonOnlyOnce">Defines with the <typeparamref name="TRegistrar"/> how to register a type to it self as a singleton and makes sure it is only registered once.</param>
    /// <param name="registerSingletonOnlyOnce">Defines with the <typeparamref name="TRegistrar"/> how to register a service type to the implementation type as a singleton and makes sure it is only registered once.</param>
    /// <param name="registerInstance">Defines with the <typeparamref name="TRegistrar"/> how to register a service type with its instance already created.</param>
    /// <param name="registerMapping">Defines with the <typeparamref name="TRegistrar"/> how to register a service type with its implementing type already registered but should be forwarded to this service type.</param>
    /// <param name="registerMappingOnlyOnce">Defines with the <typeparamref name="TRegistrar"/> how to register a service type with its implementing type already registered but should be forwarded to this service type and makes sure that its mapping is only registered once.</param>
    /// <param name="registerTransient">Defines with the <typeparamref name="TRegistrar"/> how to register a service type to the implementation type as a transient.</param>
    /// <param name="registerTransientOnlyOnce">Defines with the <typeparamref name="TRegistrar"/> how to register a service type to the implementation type as a transient and makes sure that it is only registered once.</param>
    public DependencyInjectionRegistrarAdapter(
        TRegistrar registrar,
        Action<TRegistrar, Type, Type> registerSingletonOnlyOnce,
        Action<TRegistrar, Type> registerSelfSingletonOnlyOnce,
        Action<TRegistrar, Type, object> registerInstance,
        Action<TRegistrar, Type, Type> registerMapping,
        Action<TRegistrar, Type, Type> registerMappingOnlyOnce,
        Action<TRegistrar, Type, Type> registerTransient,
        Action<TRegistrar, Type, Type> registerTransientOnlyOnce)
    {
        Registrar = registrar;
        RegisterSelfSingletonOnlyOnce = registerSelfSingletonOnlyOnce;
        RegisterSingletonOnlyOnce = registerSingletonOnlyOnce;
        RegisterInstance = registerInstance;
        RegisterMapping = registerMapping;
        RegisterMappingOnlyOnce = registerMappingOnlyOnce;
        RegisterTransient = registerTransient;
        RegisterTransientOnlyOnce = registerTransientOnlyOnce;
    }

    internal void Register(
        MediatRServiceConfiguration<TRegistrar> configuration,
        Type implementingType,
        IEnumerable<Type> serviceTypes,
        bool mustOnlyRegisterOnce)
    {
        if (configuration.RegistrationOptions == RegistrationOptions.Transient)
        {
            foreach (var serviceType in serviceTypes)
            {
                if (mustOnlyRegisterOnce)
                {
                    RegisterTransientOnlyOnce(Registrar, serviceType, implementingType);
                }
                else
                {
                    RegisterTransient(Registrar, serviceType, implementingType);
                }
            }
        }
        else
        {
            RegisterSelfSingletonOnlyOnce(Registrar, implementingType);
            foreach (var serviceType in serviceTypes)
            {
                if (mustOnlyRegisterOnce)
                {
                    RegisterMappingOnlyOnce(Registrar, serviceType, implementingType);
                }
                else
                {
                    RegisterMapping(Registrar, serviceType, implementingType);
                }
            }
        }
    }

    internal void RegisterSingleton(Type implementationType, IEnumerable<Type> serviceTypes)
    {
        RegisterSelfSingletonOnlyOnce(Registrar, implementationType);
        foreach (var serviceType in serviceTypes)
        {
            RegisterMappingOnlyOnce(Registrar, serviceType, implementationType);
        }
    }
}