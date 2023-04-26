using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Core.Reflection.Abstractions;

namespace Core.Reflection
{
    public class StandardFuncMethod<TResult> : IStandardFuncMethod<TResult>
    {
        private Func<IServiceProvider, object, TResult> _invoke;
        public StandardFuncMethod(IEnumerable<Attribute> attributes, Func<IServiceProvider, object, TResult> invoke)
        {
            Attributes = attributes;
            _invoke = invoke;
        }

        public IEnumerable<Attribute> Attributes { get; }

        public TResult Invoke(IServiceProvider provider)
        {
            return _invoke(provider, null);
        }

        public TResult Invoke(object obj)
        {
            return _invoke(null, obj);
        }
    }
    public class StandardFuncMethod<T1, TResult> : IStandardFuncMethod<T1, TResult>
    {
        private Func<IServiceProvider, object, T1, TResult> _invoke;
        public Type T1Type { get; }
        public StandardFuncMethod(Type t1Type, IEnumerable<Attribute> attributes, Func<IServiceProvider, object, T1, TResult> invoke)
        {
            T1Type = t1Type;
            Attributes = attributes;
            _invoke = invoke;
        }

        public IEnumerable<Attribute> Attributes { get; }

        public TResult Invoke(IServiceProvider provider, T1 arg1)
        {
            return _invoke(provider, null, arg1);
        }

        public TResult Invoke(object obj, T1 arg1)
        {
            return _invoke(null, obj, arg1);
        }
    }
    public class StandardFuncMethod<T1, T2, TResult> : IStandardFuncMethod<T1, T2, TResult>
    {
        private Func<IServiceProvider, object, T1, T2, TResult> _invoke;
        public Type T1Type { get; }
        public Type T2Type { get; }
        public StandardFuncMethod(Type t1Type, Type t2Type, IEnumerable<Attribute> attributes, Func<IServiceProvider, object, T1, T2, TResult> invoke)
        {
            T1Type = t1Type;
            T2Type = t2Type;
            Attributes = attributes;
            _invoke = invoke;
        }

        public IEnumerable<Attribute> Attributes { get; }


        public TResult Invoke(IServiceProvider provider, T1 arg1, T2 arg2)
        {
            return _invoke(provider, null, arg1, arg2);
        }

        public TResult Invoke(object obj, T1 arg1, T2 arg2)
        {
            return _invoke(null, obj, arg1, arg2);
        }
    }
}
