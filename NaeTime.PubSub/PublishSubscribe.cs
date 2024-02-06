﻿using Microsoft.Extensions.DependencyInjection;
using NaeTime.PubSub.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.PubSub;
internal class PublishSubscribe : IPublishSubscribe
{
    private readonly IServiceProvider _serviceProvider;

    private ConcurrentDictionary<Type, IEnumerable<SubscriberHandler>> _subscribers = new();
    private ConcurrentDictionary<Type, ConcurrentDictionary<Type, RequestHandler>> _classHandlers = new();

    private ConcurrentDictionary<object, ConcurrentDictionary<(Type requestType, Type responseType), int>> _instanceDynamicHandlers = new();
    private ConcurrentDictionary<Type, ConcurrentDictionary<Type, Func<object, Task<object?>>>> _dynamicHandlers = new();

    private ConcurrentDictionary<object, ConcurrentDictionary<Type, int>> _instanceTypes = new();
    private ConcurrentDictionary<Type, ConcurrentDictionary<object, Func<object, Task>>> _subscriptions = new();
    public PublishSubscribe(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }


    public async Task<TResponse?> Request<TRequest, TResponse>(TRequest request)
        where TRequest : notnull
    {
        var responseType = typeof(TResponse);
        var requestType = typeof(TRequest);

        if (_dynamicHandlers.TryGetValue(requestType, out var dynamicReponseHandlers))
        {
            if (dynamicReponseHandlers.TryGetValue(requestType, out var handler))
            {
                var response = await handler(request);

                if (response == null)
                {
                    return default;
                }

                return (TResponse)response;
            }
        }

        var classHandler = TryGetHandler(requestType, responseType);
        if (classHandler != null)
        {
            return await classHandler.Handle<TResponse>(request);
        }
#if DEBUG
        throw new NotImplementedException("Missing Handler");
#else
        return default;
#endif
    }
    private RequestHandler? TryGetHandler(Type requestType, Type responseType)
    {
        if (_classHandlers.TryGetValue(requestType, out var responseHandlers))
        {
            if (responseHandlers.TryGetValue(responseType, out var classHandler))
            {
                return classHandler;
            }
            else
            {
                var handler = CreateResponseTypeHandler(requestType, responseType);

                if (handler != null)
                {
                    responseHandlers.TryAdd(responseType, handler);
                }

                return handler;
            }
        }
        else
        {
            var handler = CreateResponseTypeHandler(requestType, responseType);

            if (handler != null)
            {
                _classHandlers.TryAdd(requestType, new ConcurrentDictionary<Type, RequestHandler>());
                _classHandlers[requestType].TryAdd(responseType, handler);
            }

            return handler;
        }
    }

    public async Task Dispatch<T>(T message)
        where T : class
    {
        var publishTasks = new List<Task>
        {
            DispatchToDynamicSubscribers(message)
        };

        var subscriberBag = _subscribers.GetOrAdd(message.GetType(), _ =>
        {
            return CreateSubscribers<T>();
        });

        foreach (var subscriber in subscriberBag)
        {
            publishTasks.Add(subscriber.Handle(message));
        }

        await Task.WhenAll(publishTasks);
    }
    private RequestHandler? CreateResponseTypeHandler(Type requestType, Type responseType)
    {
        var handlers = _serviceProvider.GetServices<IHandlerRegistration>();

        foreach (var handler in handlers)
        {
            if (handler.RequestType != requestType)
            {
                continue;
            }
            if (handler.ResponseType != responseType)
            {
                continue;
            }
            Func<object> getHandler;
            if (handler.Lifetime == ServiceLifetime.Singleton)
            {
                var instance = ActivatorUtilities.CreateInstance(_serviceProvider, handler.HandlerType);
                getHandler = () => instance;
            }
            else
            {
                getHandler = () => ActivatorUtilities.CreateInstance(_serviceProvider, handler.HandlerType);
            }

            return new RequestHandler(handler.HandlerType, requestType, responseType, getHandler);
        }

        return null;

    }
    private IEnumerable<SubscriberHandler> CreateSubscribers<T>() where T : class
    {
        var subscribers = _serviceProvider.GetServices<ISubscriberRegistration>();

        var bag = new List<SubscriberHandler>();
        var messageType = typeof(T);

        foreach (var registration in subscribers)
        {
            if (registration.MessageType != messageType)
            {
                continue;
            }

            Func<object> subscriber;
            if (registration.Lifetime == ServiceLifetime.Singleton)
            {
                var instance = ActivatorUtilities.CreateInstance(_serviceProvider, registration.SubscriberType);
                subscriber = () => instance;
            }
            else
            {
                subscriber = () => ActivatorUtilities.CreateInstance(_serviceProvider, registration.SubscriberType);
            }

            var handler = new SubscriberHandler(registration.SubscriberType, registration.MessageType, subscriber);

            bag.Add(handler);
        }

        return bag;
    }

    private Task DispatchToDynamicSubscribers(object message)
    {
        var messageType = message.GetType();

        if (!_subscriptions.TryGetValue(messageType, out var typeSubscriptions))
        {
            return Task.CompletedTask;
        }

        var tasks = new List<Task>();

        foreach (var subscription in typeSubscriptions)
        {
            tasks.Add(subscription.Value(message));
        }

        return Task.WhenAll(tasks);
    }

    public void Subscribe<TMessageType>(object subscriber, Func<TMessageType, Task> onMessage)
    {
        var typeSubscriptions = _subscriptions.GetOrAdd(typeof(TMessageType), _ => new ConcurrentDictionary<object, Func<object, Task>>());
        typeSubscriptions.TryAdd(subscriber, (message) => onMessage((TMessageType)message));

        var instanceSubscribedTypes = _instanceTypes.GetOrAdd(subscriber, _ => new ConcurrentDictionary<Type, int>());
        instanceSubscribedTypes.TryAdd(typeof(TMessageType), 0);
    }

    private bool DoesHandlerAlreadyExist(Type requestType, Type responseType)
    {
        if (_dynamicHandlers.TryGetValue(requestType, out var dynamicResponseHandlers)
            && dynamicResponseHandlers.TryGetValue(responseType, out var _))
        {
            return true;
        }

        var classHandler = TryGetHandler(requestType, responseType);

        if (classHandler != null)
        {
            return true;
        }

        return false;
    }

    public void RespondTo<TRequest, TResponse>(object subscriber, Func<TRequest, Task<TResponse?>> onRequest)
        where TResponse : class
    {
        var requestType = typeof(TRequest);
        var responseType = typeof(TResponse);

        if (DoesHandlerAlreadyExist(requestType, responseType))
        {
            throw new ArgumentException("Reponse Handler already exists");
        }

        var responseHandlers = _dynamicHandlers.GetOrAdd(requestType, _ => new ConcurrentDictionary<Type, Func<object, Task<object?>>>());

        responseHandlers.TryAdd(responseType, async x =>
        {
            var typedRequest = (TRequest)x;

            var response = await onRequest(typedRequest);

            return response;
        });


        var handlers = _instanceDynamicHandlers.GetOrAdd(subscriber, _ => new ConcurrentDictionary<(Type requestType, Type responseType), int>());
        handlers.TryAdd((requestType, responseType), 0);
    }

    public void Unsubscribe(object subscriber)
    {
        if (_instanceDynamicHandlers.TryGetValue(subscriber, out var handlers))
        {
            foreach (var handler in handlers)
            {
                if (_dynamicHandlers.TryGetValue(handler.Key.requestType, out var responseHandlers))
                {
                    responseHandlers.TryRemove(handler.Key.responseType, out var _);

                    if (responseHandlers.Count == 0)
                    {
                        _dynamicHandlers.TryRemove(handler.Key.requestType, out var _);
                    }
                }
            }

            _instanceDynamicHandlers.TryRemove(subscriber, out var _);
        }

        if (!_instanceTypes.TryGetValue(subscriber, out var instanceSubscribedTypes))
        {
            return;
        }

        foreach (var typeKvp in instanceSubscribedTypes)
        {
            if (!_subscriptions.TryGetValue(typeKvp.Key, out var typeSubscriptions))
            {
                continue;
            }

            typeSubscriptions.TryRemove(subscriber, out _);
        }
        _instanceTypes.TryRemove(subscriber, out _);
    }

}
