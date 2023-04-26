using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Core.Logging.Abstractions;
using Core.Logging.Core;

namespace Core.Logging.Core
{
    public class LoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private ConcurrentDictionary<string, ILogger> _loggers = new ConcurrentDictionary<string, ILogger>();
        public IExternalScopeProvider ScopeProvider { get; private set; }

        private readonly ILogQueue _queue;
        private readonly IEnumerable<IGlobalLogPropertyProvider> _propertyProviders;
        public LoggerProvider(ILogQueue queue, IEnumerable<IGlobalLogPropertyProvider> propertyProviders)
        {
            _queue = queue;
            _propertyProviders = propertyProviders;
        }

        protected virtual ILogger CreateLogger(string categoryName, ILogQueue queue)
        {
            return new Logger(
                category: categoryName,
                queue: queue,
                propertyProviders: _propertyProviders,
                provider: this);
        }
        public virtual bool IsEnabled(LogLevel level)
        {
            return true;
        }

        public virtual ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, x =>
            {
                return CreateLogger(x, _queue);
            });
        }

        public void Dispose()
        {
            _loggers.Clear();
        }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            ScopeProvider = scopeProvider;
        }
    }
}
