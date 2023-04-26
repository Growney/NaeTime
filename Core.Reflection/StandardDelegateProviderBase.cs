using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Core.Reflection.Abstractions;

namespace Core.Reflection
{
    public abstract class StandardDelegateProviderBase
    {
        private readonly MethodStandard _standard;
        private readonly string _standardMethodName;
        private readonly ILogger _logger;

        public StandardDelegateProviderBase(ILogger logger, MethodStandard standard)
        {
            _standard = standard;
            _logger = logger;
            _standardMethodName = _standard.IsCaseSensitive ? _standard.Name : _standard.Name?.ToLower();
        }

        private IEnumerable<MethodInfo> GetStandardMethods(Type type)
        {
            var allMethods = type.GetMethods();
            foreach (var method in allMethods)
            {
                if (DoesMethodMeetRules(type, method))
                {
                    yield return method;
                }
            }
        }
        private bool DoesMethodMeetRules(Type objectType, MethodInfo info)
        {
            if (!IsValidStandardMethod(objectType, info))
            {
                _logger.LogTrace("Method {methodName} of type {typeName} skipped as it is not a valid standard method", info.Name, objectType.Name);
                return false;
            }
            if (!DoesMethodMeetNameRules(info))
            {
                _logger.LogTrace("Method {methodName} of type {typeName} skipped as does not meet the naming standard", info.Name, objectType.Name);
                return false;
            }

            _logger.LogTrace("Method {methodName} of type {typeName} meets standards and naming rules", info.Name, objectType.Name);
            return true;
        }
        private bool IsValidStandardMethod(Type objectType, MethodInfo info)
        {
            if (info.IsConstructor)
            {
                _logger.LogTrace("Method {methodName} of type {typeName} skipped as it is a constructor", info.Name, objectType.Name);
                return false;
            }
            if (info.IsStatic)
            {
                _logger.LogTrace("Method {methodName} of type {typeName} skipped as it is static", info.Name, objectType.Name);
                return false;
            }
            if (info.IsGenericMethod)
            {
                _logger.LogTrace("Method {methodName} of type {typeName} skipped as it is generic", info.Name, objectType.Name);
                return false;
            }
            if (!_standard.AllowPrivateMethods)
            {
                if (info.IsPrivate)
                {
                    _logger.LogTrace("Method {methodName} of type {typeName} skipped as it is private", info.Name, objectType.Name);
                    return false;
                }
            }

            if (!_standard.AllowInheritedMethods)
            {
                if (objectType != info.DeclaringType)
                {
                    _logger.LogTrace("Method {methodName} of type {typeName} skipped as it is not implemented by the declaring type", info.Name, objectType.Name);
                    return false;
                }
            }

            _logger.LogTrace("Method {methodName} of type {typeName} meets standard method rules", info.Name, objectType.Name);
            return true;
        }
        private bool DoesMethodMeetNameRules(MethodInfo info)
        {
            var methodName = _standard.IsCaseSensitive ? info.Name : info.Name.ToLower();
            if (_standardMethodName == null || methodName == _standardMethodName)
            {
                return true;
            }
            else
            {
                var methodAttributes = info.GetCustomAttributes();
                foreach (var attribute in methodAttributes)
                {
                    if (_standard.OverrideAttributes.Contains(attribute.GetType()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool DoesParametersMeetStandard(Type[] methodParameters, Type[] standardParameters, Dictionary<int, bool> required = null)
        {
            if (!_standard.AllowMissingParameters && methodParameters.Length != standardParameters.Length)
            {
                return false;
            }

            var parameterOrdinals = GetParameterOrdinals(methodParameters, standardParameters);

            if (_standard.IsOrderSensitive)
            {
                //Loop to check that the ordinals go in ascending order
                int currentHighestOrdinal = -1;
                for (int i = 0; i < parameterOrdinals.Length; i++)
                {
                    if (parameterOrdinals[i] > currentHighestOrdinal)
                    {
                        currentHighestOrdinal = parameterOrdinals[i];
                    }
                    else if (parameterOrdinals[i] != -1)
                    {
                        _logger.LogTrace("Method skipped as parameters are not in correct order");
                        return false;
                    }
                }
            }
            if (required != null)
            {
                for (int i = 0; i < standardParameters.Length; i++)
                {
                    if (required.TryGetValue(i, out bool isParameterRequired))
                    {
                        if (isParameterRequired)
                        {
                            if (parameterOrdinals[i] == -1)
                            {
                                _logger.LogTrace("Method skipped as required parameter is missing");
                                return false;
                            }
                        }
                    }
                }
            }

            var methodOrdinals = GetParameterOrdinals(standardParameters, methodParameters);
            //Check to see if all the method parameters can be fufilled.
            for (int i = 0; i < methodOrdinals.Length; i++)
            {
                if (methodOrdinals[i] == -1)
                {
                    _logger.LogTrace("Method skipped as not all method parameters can be fufilled");
                    return false;
                }
            }

            return true;

        }
        private int[] GetParameterOrdinals(Type[] methodParameters, Type[] standardParameters)
        {
            int[] parameterOrdinals = new int[standardParameters.Length];
            HashSet<int> usedOrdinals = new HashSet<int>();
            for (int parameterIndex = 0; parameterIndex < standardParameters.Length; parameterIndex++)
            {
                var standardParameterType = standardParameters[parameterIndex];
                int parameterOrdinal = -1;
                for (int methodParameterIndex = 0; methodParameterIndex < methodParameters.Length; methodParameterIndex++)
                {
                    if (!usedOrdinals.Contains(methodParameterIndex))
                    {
                        var methodParameterType = methodParameters[methodParameterIndex];
                        if (DoTypesMatch(methodParameterType, standardParameterType, _standard.AllowParameterInheritance))
                        {
                            parameterOrdinal = methodParameterIndex;
                            usedOrdinals.Add(parameterOrdinal);
                            break;
                        }
                    }
                }

                parameterOrdinals[parameterIndex] = parameterOrdinal;
            }
            return parameterOrdinals;
        }
        private bool DoTypesMatch(Type type1, Type type2, bool allowInheritance)
        {
            if (!allowInheritance)
            {
                return type1 == type2;
            }
            else
            {
                if (type2.IsInterface)
                {
                    return type1.ImplementsInterface(type2);
                }
                else
                {
                    return type1.IsAssignableFrom(type2) || type1 == type2;
                }
            }
        }
        private bool DoesReturnTypeMatch(Type methodReturnType, Type standardReturnType)
        {
            if (methodReturnType == typeof(void) && standardReturnType == typeof(void))
            {
                _logger.LogTrace("Return types match as they are both null");
                return true;
            }
            else if (standardReturnType == typeof(void) && methodReturnType != typeof(void))
            {
                if (_standard.AllowDiscardedReturnTypes)
                {
                    _logger.LogTrace("Return types match as discard return types is allowed");
                    return true;
                }
            }
            else if (standardReturnType == typeof(Task))
            {
                if (methodReturnType == typeof(Task))
                {
                    _logger.LogTrace("Return types match as they are both tasks");
                    return true;
                }

                if (_standard.ShouldWrapSynchronousMethods)
                {
                    if (methodReturnType == typeof(void) || _standard.AllowDiscardedReturnTypes)
                    {
                        _logger.LogTrace("Return types match as they are both tasks");
                        return true;
                    }
                }
            }
            else if (standardReturnType.IsGenericType && standardReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var standardTaskReturn = standardReturnType.GenericTypeArguments[0];
                if (methodReturnType.IsGenericType && methodReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var methodTaskReturn = methodReturnType.GenericTypeArguments[0];

                    if (DoTypesMatch(methodTaskReturn, standardTaskReturn, _standard.AllowReturnTypeInheritance))
                    {
                        _logger.LogTrace("Return types match as the correct task return type is returned");
                        return true;
                    }
                }
                else if (_standard.ShouldWrapSynchronousMethods)
                {
                    if (DoTypesMatch(methodReturnType, standardTaskReturn, _standard.AllowReturnTypeInheritance))
                    {
                        _logger.LogTrace("Return types match as the return type will be wrapped into a task");
                        return true;
                    }
                }
            }
            else
            {
                if (DoTypesMatch(methodReturnType, standardReturnType, _standard.AllowReturnTypeInheritance))
                {
                    _logger.LogTrace("Return type match the allowed types");
                    return true;
                }
            }
            _logger.LogTrace("Return type failed to match");
            return false;
        }
        protected object[] AlignParameters(ParameterInfo[] methodParameters, Type[] standardParameters, object[] parameterValues)
        {
            object[] parameters = new object[methodParameters.Length];
            var parameterOrdinals = GetParameterOrdinals(methodParameters.GetParameterTypes(), standardParameters);
            for (int i = 0; i < parameterOrdinals.Length; i++)
            {
                var ordinal = parameterOrdinals[i];
                if (ordinal != -1)
                {
                    parameters[ordinal] = parameterValues[i];
                }
            }
            return parameters;
        }
        private Dictionary<int, Type> CreateParameterMap(Type[] methodParameters, Type[] standardParameters)
        {
            var map = new Dictionary<int, Type>();
            var ordinals = GetParameterOrdinals(methodParameters, standardParameters);
            for (int i = 0; i < standardParameters.Length; i++)
            {
                var parameterIndex = ordinals[i];
                if (parameterIndex != -1)
                {
                    map.Add(i, methodParameters[parameterIndex]);
                }
            }

            return map;
        }

        protected IEnumerable<TMethodType> GetMethods<TMethodType>(Type objectType, Type standardReturnType, Type[] standardParameters, Func<Dictionary<int, Type>, MethodInfo, TMethodType> createMethod, Dictionary<int, bool> requiredParameters = null)
        {
            using (var loggerScope = CreateLoggerScope())
            {
                var methods = GetStandardMethods(objectType);
                foreach (var method in methods)
                {
                    var parameterTypes = method.GetParameters().GetParameterTypes();
                    if (DoesParametersMeetStandard(parameterTypes, standardParameters, requiredParameters))
                    {
                        if (DoesReturnTypeMatch(method.ReturnType, standardReturnType))
                        {
                            var paramaterMap = CreateParameterMap(parameterTypes, standardParameters);

                            yield return createMethod(paramaterMap, method);
                        }
                    }
                }
            }
        }

        private IDisposable CreateLoggerScope()
        {
            return _logger.BeginScope(CreateScopeValues());
        }

        private IEnumerable<KeyValuePair<string, object>> CreateScopeValues()
        {
            return new List<KeyValuePair<string, object>>()
            {
                new KeyValuePair<string, object>("Name",_standard.Name),
                new KeyValuePair<string, object>("AllowInheritedMethods",_standard.AllowInheritedMethods),
                new KeyValuePair<string, object>("AllowPrivateMethods",_standard.AllowPrivateMethods),
                new KeyValuePair<string, object>("IsCaseSensitive",_standard.IsCaseSensitive),
                new KeyValuePair<string, object>("IsOrderSensitive",_standard.IsOrderSensitive),
                new KeyValuePair<string, object>("AllowMissingParameters",_standard.AllowMissingParameters),
                new KeyValuePair<string, object>("AllowParameterInheritance",_standard.AllowParameterInheritance),
                new KeyValuePair<string, object>("ShouldWrapSynchronousMethods",_standard.ShouldWrapSynchronousMethods),
                new KeyValuePair<string, object>("AllowReturnTypeInheritance",_standard.AllowReturnTypeInheritance),
                new KeyValuePair<string, object>("AllowDiscardedReturnTypes",_standard.AllowDiscardedReturnTypes),
            };
        }
    }
}
