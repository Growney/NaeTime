using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.Reflection.Abstractions
{
    public class MethodStandard
    {
        public MethodStandard(string name = null,
            bool allowPrivateMethods = true,
            bool allowInheritedMethods = false,
            bool isCaseSensitive = true,
            bool isOrderSensitive = false,
            bool allowMissingParameters = true,
            bool allowParameterInheritance = true,
            bool shouldWrapSynchronousMethods = true,
            bool allowReturnTypeInheritance = true,
            bool allowDiscardedReturnTypes = true,
            params Type[] overrideAttributes)
        {
            Name = name;
            AllowPrivateMethods = allowPrivateMethods;
            AllowInheritedMethods = allowInheritedMethods;
            IsCaseSensitive = isCaseSensitive;
            IsOrderSensitive = isOrderSensitive;
            AllowMissingParameters = allowMissingParameters;
            AllowParameterInheritance = allowParameterInheritance;
            ShouldWrapSynchronousMethods = shouldWrapSynchronousMethods;
            AllowDiscardedReturnTypes = allowDiscardedReturnTypes;
            AllowReturnTypeInheritance = allowReturnTypeInheritance;
            OverrideAttributes = overrideAttributes;
        }

        /// <summary>
        /// The name of the standard method, when value is null any method matching the return type and parameters will meet the standard
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// True when methods can be taken from classes the class is derived from
        /// </summary>
        public bool AllowInheritedMethods { get; }
        /// <summary>
        /// True when the standard should match on public and private methods
        /// </summary>
        public bool AllowPrivateMethods { get; }
        /// <summary>
        /// True when the method name should be treated as case sensitive
        /// </summary>
        public bool IsCaseSensitive { get; }
        /// <summary>
        /// True when the order of the parameters should match the requested order
        /// </summary>
        public bool IsOrderSensitive { get; }
        /// <summary>
        /// True when the standard allows parameters to be missing
        /// </summary>
        public bool AllowMissingParameters { get; }
        /// <summary>
        /// True when parameter types do not have to be an exact match but instead can be boxed to the required type
        /// </summary>
        public bool AllowParameterInheritance { get; }
        /// <summary>
        /// True when requested method returns a Task<T> methods that match the standard but do not return a task will be wrapped to return a task.
        /// </summary>
        public bool ShouldWrapSynchronousMethods { get; }
        /// <summary>
        /// True when the return type does not have to be an exact match but can instead be boxed to the required type
        /// </summary>
        public bool AllowReturnTypeInheritance { get; }

        /// <summary>
        /// When true methods with return types can be used in a standard where no return type is required 
        /// </summary>
        public bool AllowDiscardedReturnTypes { get; }
        /// <summary>
        /// Types of attributes that cause a method to ignore the name validation.
        /// When a method has any of the provided attribute types the method will meet the naming standard assuming the parameters and return type is relevant
        /// </summary>
        public Type[] OverrideAttributes { get; }
    }
}
