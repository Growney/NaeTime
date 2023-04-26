using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Reflection.Abstractions
{
    public interface IStandardFuncMethod<TResult>
    {
        IEnumerable<Attribute> Attributes { get; }
        TResult Invoke(object obj);
        TResult Invoke(IServiceProvider provider);
    }
    public interface IStandardFuncMethod<T1, TResult>
    {
        IEnumerable<Attribute> Attributes { get; }
        Type T1Type { get; }
        TResult Invoke(object obj, T1 arg1);
        TResult Invoke(IServiceProvider provider, T1 arg1);
    }
    public interface IStandardFuncMethod<T1, T2, TResult>
    {
        IEnumerable<Attribute> Attributes { get; }
        Type T1Type { get; }
        Type T2Type { get; }
        TResult Invoke(object obj, T1 arg1, T2 arg2);
        TResult Invoke(IServiceProvider provider, T1 arg1, T2 arg2);
    }
}
