using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Core.Reflection.Abstractions;

namespace Core.Reflection
{
    public class MethodProviderFactory : IMethodProviderFactory
    {
        private readonly IServiceProvider _provider;
        public MethodProviderFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public IStandardActionProvider<T1, T2> GetActionProvider<T1, T2>(MethodStandard standard)
        {
            var logger = _provider.GetService<ILogger<StandardActionProvider<T1, T2>>>();
            return new StandardActionProvider<T1, T2>(_provider, logger, standard);
        }

        public IStandardActionProvider<T1> GetActionProvider<T1>(MethodStandard standard)
        {
            var logger = _provider.GetService<ILogger<StandardActionProvider<T1>>>();
            return new StandardActionProvider<T1>(_provider, logger, standard);
        }

        public IStandardActionProvider GetActionProvider(MethodStandard standard)
        {
            var logger = _provider.GetService<ILogger<StandardActionProvider>>();
            return new StandardActionProvider(_provider, logger, standard);
        }

        public IStandardFuncProvider<T1, T2, TResult> GetFuncProvider<T1, T2, TResult>(MethodStandard standard)
        {
            var logger = _provider.GetService<ILogger<StandardFuncProvider<T1, T2, TResult>>>();
            return new StandardFuncProvider<T1, T2, TResult>(_provider, logger, standard);
        }

        public IStandardFuncProvider<T1, TResult> GetFuncProvider<T1, TResult>(MethodStandard standard)
        {
            var logger = _provider.GetService<ILogger<StandardFuncProvider<T1, TResult>>>();
            return new StandardFuncProvider<T1, TResult>(_provider, logger, standard);
        }

        public IStandardFuncProvider<TResult> GetFuncProvider<TResult>(MethodStandard standard)
        {
            var logger = _provider.GetService<ILogger<StandardFuncProvider<TResult>>>();
            return new StandardFuncProvider<TResult>(_provider, logger, standard);
        }
    }
}
