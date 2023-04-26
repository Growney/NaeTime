using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Core.Logging.Abstractions;

namespace Core.Logging.Core
{
    public class Logger : ILogger
    {
        private readonly LoggerProvider _provider;
        private readonly string _category;
        private readonly ILogQueue _queue;
        private readonly IEnumerable<IGlobalLogPropertyProvider> _propertyProviders;

        public Logger(LoggerProvider provider, IEnumerable<IGlobalLogPropertyProvider> propertyProviders, ILogQueue queue, string category)
        {
            _provider = provider;
            _queue = queue;
            _category = category;
            _propertyProviders = propertyProviders;
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return _provider.ScopeProvider.Push(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            if (_provider == null)
            {
                return true;
            }
            return _provider.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                LogItem item = new LogItem()
                {
                    Level = logLevel,
                    Id = eventId.Id,
                    Name = eventId.Name,
                    Category = _category,
                    Message = state?.ToString() ?? "No State",
                    Exception = CreateException(exception)
                };

                if (state is IEnumerable<KeyValuePair<string, object>> values)
                {
                    List<LogProperty> properties = new List<LogProperty>();
                    foreach (var value in values)
                    {
                        if (value.Key != null)
                        {
                            properties.Add(new LogProperty() { Text = value.Key, Value = value.Value?.ToString() ?? "null" });
                        }
                    }
                    item.Properties = properties;
                }


                List<LogScope> scopes = new List<LogScope>();

                if (_propertyProviders != null)
                {
                    var globalProperties = new List<LogProperty>();
                    foreach (var provider in _propertyProviders)
                    {
                        var providerProperties = provider?.GetProperties(logLevel);

                        if (providerProperties != null)
                        {
                            foreach (var value in providerProperties)
                            {
                                if (value.Key != null)
                                {
                                    globalProperties.Add(new LogProperty() { Text = value.Key, Value = value.Value?.ToString() ?? "null" });
                                }
                            }
                        }
                    }
                    if (globalProperties.Count > 0)
                    {
                        scopes.Add(new LogScope { Properties = globalProperties });
                    }
                }

                if (_provider.ScopeProvider != null)
                {
                    _provider.ScopeProvider.ForEachScope((value, prop) =>
                    {

                        List<LogProperty> properties = new List<LogProperty>();
                        switch (value)
                        {
                            case string str:
                                {
                                    properties.Add(new LogProperty() { Text = "Text", Value = str });
                                }
                                break;
                            case IEnumerable<KeyValuePair<string, object>> props:
                                {
                                    foreach (var property in props)
                                    {
                                        properties.Add(new LogProperty() { Text = property.Key, Value = property.Value?.ToString() ?? "Null" });
                                    }
                                }
                                break;
                            default:
                                break;
                        }

                        scopes.Add(new LogScope() { Properties = properties });

                    }, state);
                }


                LogEntry entry = new LogEntry()
                {
                    DateTime = DateTime.UtcNow,
                    Item = item,
                    Scopes = scopes
                };


                _queue.Enqueue(entry);
            }
        }

        private LogException CreateException(Exception exception)
        {
            return exception != null ? new LogException()
            {
                InnerException = exception.InnerException != null ? CreateException(exception.InnerException) : null,
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                Type = exception.GetType().ToString(),
                Source = exception.Source
            } : null;
        }
    }
}
