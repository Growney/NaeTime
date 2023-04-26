using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Core.Reflection.Abstractions;

namespace Core.Reflection
{
    public class StandardActionMethod : IStandardActionMethod
    {
        private Action<IServiceProvider, object> _invoke;
        public StandardActionMethod(IEnumerable<Attribute> attributes, Action<IServiceProvider, object> invoke)
        {
            Attributes = attributes;
            _invoke = invoke;
        }

        public IEnumerable<Attribute> Attributes { get; }


        public void Invoke(IServiceProvider provider)
        {
            _invoke(provider, null);
        }

        public void Invoke(object obj)
        {
            _invoke(null, obj);
        }
    }
    public class StandardActionMethod<T1> : IStandardActionMethod<T1>
    {
        private Action<IServiceProvider, object, T1> _invoke;
        public Type T1Type { get; }
        public StandardActionMethod(Type t1Type, IEnumerable<Attribute> attributes, Action<IServiceProvider, object, T1> invoke)
        {
            T1Type = t1Type;
            Attributes = attributes;
            _invoke = invoke;
        }

        public IEnumerable<Attribute> Attributes { get; }

        public void Invoke(IServiceProvider provider, T1 arg1)
        {
            _invoke(provider, null, arg1);
        }

        public void Invoke(object obj, T1 arg1)
        {
            _invoke(null, obj, arg1);
        }
    }
    public class StandardActionMethod<T1, T2> : IStandardActionMethod<T1, T2>
    {
        private Action<IServiceProvider, object, T1, T2> _invoke;
        public Type T1Type { get; }
        public Type T2Type { get; }
        public StandardActionMethod(Type t1Type, Type t2Type, IEnumerable<Attribute> attributes, Action<IServiceProvider, object, T1, T2> invoke)
        {
            T1Type = t1Type;
            T2Type = t2Type;
            Attributes = attributes;
            _invoke = invoke;
        }

        public IEnumerable<Attribute> Attributes { get; }


        public void Invoke(IServiceProvider provider, T1 arg1, T2 arg2)
        {
            _invoke(provider, null, arg1, arg2);
        }

        public void Invoke(object obj, T1 arg1, T2 arg2)
        {
            _invoke(null, obj, arg1, arg2);
        }
    }
}
