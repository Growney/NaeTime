using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Core.Reflection;
using Core.Reflection.Abstractions;
using EventSourcingCore.CommandHandler.Abstractions;

namespace EventSourcingCore.CommandHandler.Core
{
    public class StandardsBasedRegistrar : IStandardsBasedRegistrar
    {
        private readonly IMethodProviderFactory _methodProviderFactory;
        private readonly IGlobalPrecursorRegistry _globalPrecursorRegistry;
        private readonly MethodStandard _commandHandlerStandard;
        private readonly IStandardActionProvider<ICommand, CommandMetadata> _actionProvider;

        private readonly ILogger _logger;

        public StandardsBasedRegistrar(IMethodProviderFactory factory, IGlobalPrecursorRegistry registry)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            _methodProviderFactory = factory;
            _globalPrecursorRegistry = registry;

            _commandHandlerStandard = new MethodStandard(
                name: "Handle",
                overrideAttributes: new Type[] { typeof(CommandHandlerAttribute) });

            _actionProvider = _methodProviderFactory.GetActionProvider<ICommand, CommandMetadata>(_commandHandlerStandard);
        }

        public StandardsBasedRegistrar(IMethodProviderFactory factory, IGlobalPrecursorRegistry registry, ILogger<StandardsBasedRegistrar> logger)
            : this(factory, registry)
        {
            _logger = logger;
        }

        private IEnumerable<ICommandPrecursor> GetOrderedPrecursors(Type declaringType, IEnumerable<Attribute> methodAttributes)
        {
            _logger?.LogTrace("Getting ordered precursors for {commandContainerType}", declaringType.Name);

            List<ICommandPrecursor> retVal = new List<ICommandPrecursor>();

            if (_globalPrecursorRegistry != null)
            {
                retVal.AddRange(_globalPrecursorRegistry.GetOrderedPrecursors());
            }

            retVal.AddRange(GetPrecursors(declaringType.GetCustomAttributes()));

            retVal.AddRange(GetPrecursors(methodAttributes));

            _logger?.LogTrace("{precursorCount} precursors found for {commandContainerType}", retVal.Count, declaringType.Name);
            return retVal;
        }

        private IEnumerable<ICommandPrecursor> GetPrecursors(IEnumerable<Attribute> attributes)
        {

            List<ICommandPrecursor> retVal = new List<ICommandPrecursor>();

            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    if (attribute is PrecursorAttribute precursorAttribute)
                    {
                        ICommandPrecursor precursor = precursorAttribute.GetPrecursor();
                        if (precursor != null)
                        {
                            retVal.Add(precursor);
                        }
                    }
                }
            }

            return retVal;
        }
        private string GetStandardMethodIdentifier(IStandardFuncMethod<ICommand, CommandMetadata, Task> method)
        {
            foreach (var methodAttribute in method.Attributes)
            {
                if (methodAttribute is CommandHandlerAttribute handlerAttribute)
                {
                    return handlerAttribute.Identifier;
                }
            }

            var commandType = method.T1Type;
            var commandAttribute = commandType.GetCustomAttribute<CommandAttribute>();

            return commandAttribute?.Identifier ?? commandType.Name;
        }
        public IEnumerable<ICommandHandler> CreateClassHandlers<T>()
        {
            return CreateClassHandlers(typeof(T));
        }
        public IEnumerable<ICommandHandler> CreateClassHandlers(Type createFor)
        {
            _logger?.LogInformation("Creating command handlers for transient container {commandContainerType}", createFor.Name);
            List<ICommandHandler> handlers = new List<ICommandHandler>();

            var provider = _methodProviderFactory.GetActionProvider<ICommand, CommandMetadata>(_commandHandlerStandard);
            var standardMethods = provider.GetAsyncMethods(createFor, t1Required: true);

            foreach (var method in standardMethods)
            {
                string identifier = GetStandardMethodIdentifier(method);
                IEnumerable<ICommandPrecursor> precursors = GetOrderedPrecursors(createFor, method.Attributes);
                _logger?.LogTrace("Found {commandIdentifier} handler in {commandContainerType} with {precursorCount} precursors", identifier, createFor.Name, precursors.Count());
                handlers.Add(new StandardFuncMethodCommandHandler(identifier, precursors, method));
            }

            _logger?.LogInformation("{handlerCount} command handlers found for transient container {commandContainerType}", handlers.Count, createFor.Name);

            return handlers;
        }
        public IEnumerable<ICommandHandler> CreateObjectHandlers(object instance)
        {
            _logger?.LogInformation("Creating command handlers for singleton container {commandContainerType}", instance.GetType().Name);
            List<ICommandHandler> handlers = new List<ICommandHandler>();

            var instanceType = instance.GetType();

            var provider = _methodProviderFactory.GetActionProvider<ICommand, CommandMetadata>(_commandHandlerStandard);
            var standardMethods = provider.GetAsyncMethods(instanceType, t1Required: true);

            foreach (var method in standardMethods)
            {
                string identifier = GetStandardMethodIdentifier(method);
                IEnumerable<ICommandPrecursor> precursors = GetOrderedPrecursors(instanceType, method.Attributes);
                _logger?.LogTrace("Found {commandIdentifier} handler in {commandContainerType} with {precursorCount} precursors", identifier, handlers.Count, instance.GetType().Name, precursors.Count());
                handlers.Add(new SingleInstanceStandardFuncMethodCommandHandler(instance, identifier, precursors, method));
            }

            _logger?.LogInformation("{handlerCount} command handlers found for singleton container {commandContainerType}", handlers.Count, instance.GetType().Name);
            return handlers;
        }
    }
}
