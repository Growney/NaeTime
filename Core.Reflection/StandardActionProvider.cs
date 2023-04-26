using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Core.Reflection;
using Core.Reflection.Abstractions;

namespace Core.Reflection
{
    public class StandardActionProvider : StandardDelegateProviderBase, IStandardActionProvider
    {
        private readonly IServiceProvider _provider;
        public StandardActionProvider(IServiceProvider provider, ILogger<StandardActionProvider> logger, MethodStandard standard) : base(logger, standard)
        {
            _provider = provider;
        }
        public IEnumerable<IStandardFuncMethod<Task>> GetAsyncMethods(Type type)
        {
            return GetMethods(
                objectType: type,
                standardReturnType: typeof(Task),
                standardParameters: new Type[0],
                createMethod: (map, method) =>
                {
                    var attributes = method.GetCustomAttributes();

                    return new StandardFuncMethod<Task>(attributes, (provider, on) =>
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
                            if (result is Task task)
                            {
                                return task;
                            }
                            else
                            {
                                return Task.CompletedTask;
                            }
                        }
                        catch (TargetInvocationException ex)
                        {
                            throw ex.InnerException ?? ex;
                        }
                    });
                });
        }
        public IEnumerable<IStandardActionMethod> GetMethods(Type type)
        {
            return GetMethods(
                objectType: type,
                standardReturnType: typeof(void),
                standardParameters: new Type[0],
                createMethod: (map, method) =>
                {
                    var attributes = method.GetCustomAttributes();
                    return new StandardActionMethod(attributes, (provider, on) =>
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
                            method.Invoke(obj, new object[0]);
                        }
                        catch (TargetInvocationException ex)
                        {
                            throw ex.InnerException ?? ex;
                        }
                    });
                });
        }
    }

    public class StandardActionProvider<T1> : StandardDelegateProviderBase, IStandardActionProvider<T1>
    {
        private readonly IServiceProvider _provider;
        public StandardActionProvider(IServiceProvider provider, ILogger<StandardActionProvider<T1>> logger, MethodStandard standard) : base(logger, standard)
        {
            _provider = provider;
        }
        public IEnumerable<IStandardFuncMethod<T1, Task>> GetAsyncMethods(Type type, bool t1Required = false)
        {
            var required = new Dictionary<int, bool>() { { 1, t1Required } };
            return GetMethods(
                objectType: type,
                standardReturnType: typeof(Task),
                standardParameters: new Type[] { typeof(T1) },
                requiredParameters: required,
                createMethod: (map, method) =>
                {
                    var attributes = method.GetCustomAttributes();

                    map.TryGetValue(0, out var t1Type);

                    return new StandardFuncMethod<T1, Task>(t1Type, attributes, (provider, on, arg1) =>
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
                            var result = method.Invoke(obj, new object[] { arg1 });
                            if (result is Task task)
                            {
                                return task;
                            }
                            else
                            {
                                return Task.CompletedTask;
                            }
                        }
                        catch (TargetInvocationException ex)
                        {
                            throw ex.InnerException ?? ex;
                        }

                    });
                });
        }
        public IEnumerable<IStandardActionMethod<T1>> GetMethods(Type type, bool t1Required = false)
        {
            var required = new Dictionary<int, bool>() { { 1, t1Required } };
            return GetMethods(
                objectType: type,
                standardReturnType: typeof(void),
                standardParameters: new Type[] { typeof(T1) },
                requiredParameters: required,
                createMethod: (map, method) =>
                {
                    var attributes = method.GetCustomAttributes();

                    map.TryGetValue(0, out var t1Type);

                    return new StandardActionMethod<T1>(t1Type, attributes, (provider, on, arg1) =>
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
                            method.Invoke(obj, new object[] { arg1 });
                        }
                        catch (TargetInvocationException ex)
                        {
                            throw ex.InnerException ?? ex;
                        }
                    });
                });
        }
    }

    public class StandardActionProvider<T1, T2> : StandardDelegateProviderBase, IStandardActionProvider<T1, T2>
    {
        private readonly IServiceProvider _provider;
        public StandardActionProvider(IServiceProvider provider, ILogger<StandardActionProvider<T1, T2>> logger, MethodStandard standard) : base(logger, standard)
        {
            _provider = provider;
        }
        public IEnumerable<IStandardFuncMethod<T1, T2, Task>> GetAsyncMethods(Type type, bool t1Required = false, bool t2Required = false)
        {
            var required = new Dictionary<int, bool>() { { 0, t1Required }, { 1, t2Required } };
            var standardParameters = new Type[] { typeof(T1), typeof(T2) };
            return GetMethods(
                objectType: type,
                standardReturnType: typeof(Task),
                standardParameters: standardParameters,
                requiredParameters: required,
                createMethod: (map, method) =>
                {
                    var attributes = method.GetCustomAttributes();

                    map.TryGetValue(0, out var t1Type);
                    map.TryGetValue(1, out var t2Type);

                    return new StandardFuncMethod<T1, T2, Task>(t1Type, t2Type, attributes, (provider, on, arg1, arg2) =>
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
                            if (result is Task task)
                            {
                                return task;
                            }
                            else
                            {
                                return Task.CompletedTask;
                            }
                        }
                        catch (TargetInvocationException ex)
                        {
                            throw ex.InnerException ?? ex;
                        }

                    });
                }); ;
        }
        public IEnumerable<IStandardActionMethod<T1, T2>> GetMethods(Type type, bool t1Required = false, bool t2Required = false)
        {
            var required = new Dictionary<int, bool>() { { 0, t1Required }, { 1, t2Required } };
            var standardParameters = new Type[] { typeof(T1), typeof(T2) };
            return GetMethods(
                objectType: type,
                standardReturnType: typeof(void),
                standardParameters: standardParameters,
                requiredParameters: required,
                createMethod: (map, method) =>
                {
                    var attributes = method.GetCustomAttributes();

                    map.TryGetValue(0, out var t1Type);
                    map.TryGetValue(1, out var t2Type);
                    return new StandardActionMethod<T1, T2>(t1Type, t2Type, attributes, (provider, on, arg1, arg2) =>
                    {
                        if (on != null)
                        {
                            if (type != on.GetType())
                            {
                                throw new ArgumentException("Type mismatch. Object must be of the same type as the methods were created from");
                            }
                        }
                        var obj = on ?? ActivatorUtilities.CreateInstance(provider ?? _provider, type);
                        var methodParameters = method.GetParameters();
                        var parameters = AlignParameters(methodParameters, standardParameters, new object[] { arg1, arg2 });
                        try
                        {
                            method.Invoke(obj, parameters);
                        }
                        catch (TargetInvocationException ex)
                        {
                            throw ex.InnerException ?? ex;
                        }

                    });
                });
        }
    }
}
