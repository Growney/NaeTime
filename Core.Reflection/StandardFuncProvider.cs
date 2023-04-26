using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Core.Reflection.Abstractions;

namespace Core.Reflection
{
    public class StandardFuncProvider<TResult> : StandardDelegateProviderBase, IStandardFuncProvider<TResult>
    {
        private readonly IServiceProvider _provider;
        public StandardFuncProvider(IServiceProvider provider, ILogger<StandardFuncProvider<TResult>> logger, MethodStandard standard) : base(logger, standard)
        {
            _provider = provider;
        }
        public IEnumerable<IStandardFuncMethod<Task<TResult>>> GetAsyncMethods(Type type)
        {
            return GetMethods(
                objectType: type,
                standardReturnType: typeof(Task<TResult>),
                standardParameters: new Type[0],
                createMethod: (map, method) =>
                {
                    var attributes = method.GetCustomAttributes();

                    return new StandardFuncMethod<Task<TResult>>(attributes, (provider, on) =>
                    {
                        if (on != null)
                        {
                            if (type != on.GetType())
                            {
                                throw new ArgumentException("Type mismatch. Object must be of the same type as the methods were created from");
                            }
                        }
                        var obj = on ?? ActivatorUtilities.CreateInstance(_provider, type);
                        try
                        {
                            var result = method.Invoke(obj, new object[0]);
                            if (result is Task<TResult> task)
                            {
                                return task;
                            }
                            else
                            {
                                return Task.FromResult((TResult)result);
                            }
                        }
                        catch (TargetInvocationException ex)
                        {
                            throw ex?.InnerException ?? ex;
                        }

                    });
                });
        }
        public IEnumerable<IStandardFuncMethod<TResult>> GetMethods(Type type)
        {
            return GetMethods(
                objectType: type,
                standardReturnType: typeof(TResult),
                standardParameters: new Type[0],
                createMethod: (map, method) =>
                {
                    var attributes = method.GetCustomAttributes();

                    return new StandardFuncMethod<TResult>(attributes, (provider, on) =>
                    {
                        if (on != null)
                        {
                            if (type != on.GetType())
                            {
                                throw new ArgumentException("Type mismatch. Object must be of the same type as the methods were created from");
                            }
                        }

                        var obj = on ?? ActivatorUtilities.CreateInstance(provider ?? _provider, type);
                        try
                        {
                            var result = method.Invoke(obj, new object[0]);

                            return (TResult)result;
                        }
                        catch (TargetInvocationException ex)
                        {
                            throw ex.InnerException ?? ex;
                        }
                    });
                });
        }
    }
    public class StandardFuncProvider<T1, TResult> : StandardDelegateProviderBase, IStandardFuncProvider<T1, TResult>
    {
        private readonly IServiceProvider _provider;
        public StandardFuncProvider(IServiceProvider provider, ILogger<StandardFuncProvider<T1, TResult>> logger, MethodStandard standard) : base(logger, standard)
        {
            _provider = provider;
        }
        public IEnumerable<IStandardFuncMethod<T1, Task<TResult>>> GetAsyncMethods(Type type, bool t1Required = false)
        {
            var required = new Dictionary<int, bool>() { { 1, t1Required } };
            return GetMethods(
                objectType: type,
                standardReturnType: typeof(Task<TResult>),
                standardParameters: new Type[] { typeof(T1) },
                requiredParameters: required,
                createMethod: (map, method) =>
                {
                    var attributes = method.GetCustomAttributes();

                    map.TryGetValue(0, out var t1Type);

                    return new StandardFuncMethod<T1, Task<TResult>>(t1Type, attributes, (provider, on, arg1) =>
                    {
                        var obj = on ?? ActivatorUtilities.CreateInstance(provider ?? _provider, type);
                        try
                        {
                            var result = method.Invoke(obj, new object[] { arg1 });
                            if (result is Task<TResult> task)
                            {
                                return task;
                            }
                            else
                            {
                                return Task.FromResult((TResult)result);
                            }
                        }
                        catch (TargetInvocationException ex)
                        {
                            throw ex?.InnerException ?? ex;
                        }
                    });
                });
        }
        public IEnumerable<IStandardFuncMethod<T1, TResult>> GetMethods(Type type, bool t1Required = false)
        {
            var required = new Dictionary<int, bool>() { { 1, t1Required } };
            return GetMethods(
                objectType: type,
                standardReturnType: typeof(TResult),
                standardParameters: new Type[] { typeof(T1) },
                requiredParameters: required,
                createMethod: (map, method) =>
                {
                    var attributes = method.GetCustomAttributes();

                    map.TryGetValue(0, out var t1Type);

                    return new StandardFuncMethod<T1, TResult>(t1Type, attributes, (provider, on, arg1) =>
                    {
                        var obj = on ?? ActivatorUtilities.CreateInstance(provider ?? _provider, type);
                        try
                        {
                            var result = method.Invoke(obj, new object[] { arg1 });
                            return (TResult)result;
                        }
                        catch (TargetInvocationException ex)
                        {
                            throw ex?.InnerException ?? ex;
                        }
                    });
                });
        }
    }
    public class StandardFuncProvider<T1, T2, TResult> : StandardDelegateProviderBase, IStandardFuncProvider<T1, T2, TResult>
    {
        private readonly IServiceProvider _provider;
        public StandardFuncProvider(IServiceProvider provider, ILogger<StandardFuncProvider<T1, T2, TResult>> logger, MethodStandard standard) : base(logger, standard)
        {
            _provider = provider;
        }
        public IEnumerable<IStandardFuncMethod<T1, T2, Task<TResult>>> GetAsyncMethods(Type type, bool t1Required = false, bool t2Required = false)
        {
            var required = new Dictionary<int, bool>() { { 0, t1Required }, { 1, t2Required } };
            var standardParameters = new Type[] { typeof(T1), typeof(T2) };
            return GetMethods(
                objectType: type,
                standardReturnType: typeof(Task<TResult>),
                standardParameters: standardParameters,
                requiredParameters: required,
                createMethod: (map, method) =>
                {
                    var attributes = method.GetCustomAttributes();

                    map.TryGetValue(0, out var t1Type);
                    map.TryGetValue(1, out var t2Type);

                    return new StandardFuncMethod<T1, T2, Task<TResult>>(t1Type, t2Type, attributes, (provider, on, arg1, arg2) =>
                    {
                        var methodParameters = method.GetParameters();
                        var obj = on ?? ActivatorUtilities.CreateInstance(provider ?? _provider, type);
                        var parameters = AlignParameters(methodParameters, standardParameters, new object[] { arg1, arg2 });
                        try
                        {
                            var result = method.Invoke(obj, parameters);
                            if (result is Task<TResult> task)
                            {
                                return task;
                            }
                            else
                            {
                                return Task.FromResult((TResult)result);
                            }
                        }
                        catch (TargetInvocationException ex)
                        {
                            throw ex?.InnerException ?? ex;
                        }
                    });
                }); ;
        }
        public IEnumerable<IStandardFuncMethod<T1, T2, TResult>> GetMethods(Type type, bool t1Required = false, bool t2Required = false)
        {
            var required = new Dictionary<int, bool>() { { 0, t1Required }, { 1, t2Required } };
            var standardParameters = new Type[] { typeof(T1), typeof(T2) };
            return GetMethods(
                objectType: type,
                standardReturnType: typeof(TResult),
                standardParameters: standardParameters,
                requiredParameters: required,
                createMethod: (map, method) =>
                {
                    var attributes = method.GetCustomAttributes();

                    map.TryGetValue(0, out var t1Type);
                    map.TryGetValue(1, out var t2Type);
                    return new StandardFuncMethod<T1, T2, TResult>(t1Type, t2Type, attributes, (provider, on, arg1, arg2) =>
                    {
                        if (on != null)
                        {
                            if (type != on.GetType())
                            {
                                throw new ArgumentException("Type mismatch. Object must be of the same type as the methods were created from");
                            }
                        }
                        var methodParameters = method.GetParameters();
                        var obj = on ?? ActivatorUtilities.CreateInstance(provider ?? _provider, type);
                        var parameters = AlignParameters(methodParameters, standardParameters, new object[] { arg1, arg2 });
                        try
                        {
                            var result = method.Invoke(obj, parameters);
                            return (TResult)result;
                        }
                        catch (TargetInvocationException ex)
                        {
                            throw ex?.InnerException ?? ex;
                        }
                    });
                });
        }
    }
}
