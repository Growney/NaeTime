using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Core.Reflection.Abstractions
{
    public interface IMethodProviderFactory
    {
        public IStandardFuncProvider<T1, T2, TResult> GetFuncProvider<T1, T2, TResult>(MethodStandard standard);
        public IStandardFuncProvider<T1, TResult> GetFuncProvider<T1, TResult>(MethodStandard standard);
        public IStandardFuncProvider<TResult> GetFuncProvider<TResult>(MethodStandard standard);

        public IStandardActionProvider<T1, T2> GetActionProvider<T1, T2>(MethodStandard standard);
        public IStandardActionProvider<T1> GetActionProvider<T1>(MethodStandard standard);
        public IStandardActionProvider GetActionProvider(MethodStandard standard);
    }
}
