using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Reflection.Abstractions
{
    public interface IStandardActionProvider
    {
        IEnumerable<IStandardActionMethod> GetMethods(Type type);
        IEnumerable<IStandardFuncMethod<Task>> GetAsyncMethods(Type type);
    }
    public interface IStandardActionProvider<T1>
    {
        IEnumerable<IStandardActionMethod<T1>> GetMethods(Type type, bool t1Required = false);
        IEnumerable<IStandardFuncMethod<T1, Task>> GetAsyncMethods(Type type, bool t1Required = false);
    }
    public interface IStandardActionProvider<T1, T2>
    {
        IEnumerable<IStandardActionMethod<T1, T2>> GetMethods(Type type, bool t1Required = false, bool t2Required = false);
        IEnumerable<IStandardFuncMethod<T1, T2, Task>> GetAsyncMethods(Type type, bool t1Required = false, bool t2Required = false);
    }
}
