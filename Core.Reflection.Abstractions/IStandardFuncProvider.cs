using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Reflection.Abstractions
{
    public interface IStandardFuncProvider<TResult>
    {
        IEnumerable<IStandardFuncMethod<TResult>> GetMethods(Type type);
        IEnumerable<IStandardFuncMethod<Task<TResult>>> GetAsyncMethods(Type type);
    }
    public interface IStandardFuncProvider<T1, TResult>
    {
        IEnumerable<IStandardFuncMethod<T1, TResult>> GetMethods(Type type, bool t1Required = false);
        IEnumerable<IStandardFuncMethod<T1, Task<TResult>>> GetAsyncMethods(Type type, bool t1Required = false);
    }
    public interface IStandardFuncProvider<T1, T2, TResult>
    {
        IEnumerable<IStandardFuncMethod<T1, T2, TResult>> GetMethods(Type type, bool t1Required = false, bool t2Required = false);
        IEnumerable<IStandardFuncMethod<T1, T2, Task<TResult>>> GetAsyncMethods(Type type, bool t1Required = false, bool t2Required = false);
    }

}
