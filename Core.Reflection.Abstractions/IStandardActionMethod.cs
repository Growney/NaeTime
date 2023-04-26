using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Reflection.Abstractions
{
    public interface IStandardActionMethod
    {
        IEnumerable<Attribute> Attributes { get; }
        void Invoke(object obj);
        void Invoke(IServiceProvider provider);
    }
    public interface IStandardActionMethod<T1>
    {
        IEnumerable<Attribute> Attributes { get; }
        Type T1Type { get; }
        void Invoke(object obj, T1 arg1);
        void Invoke(IServiceProvider provider, T1 arg1);
    }
    public interface IStandardActionMethod<T1, T2>
    {
        IEnumerable<Attribute> Attributes { get; }
        Type T1Type { get; }
        Type T2Type { get; }
        void Invoke(object obj, T1 arg1, T2 arg2);
        void Invoke(IServiceProvider provider, T1 arg1, T2 arg2);
    }
}
