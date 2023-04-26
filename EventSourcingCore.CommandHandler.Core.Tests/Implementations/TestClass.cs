using System;
using System.Threading.Tasks;
using EventSourcingCore.CommandHandler.Abstractions;

namespace EventSourcingCore.CommandHandler.Core.Tests.Implementations
{
    internal class TestClass
    {
        public int TypedMetadataAndCommandTestMethodInvokeCount { get; private set; }
        public int TypedCommandAndMetadataTestMethodInvokeCount { get; private set; }
        public int TypedCommandTestMethodInvokeCount { get; private set; }
        public int TypedAsyncMetadataAndCommandTestMethodInvokeCount { get; private set; }
        public int TypedAsyncCommandAndMetadataTestMethodInvokeCount { get; private set; }
        public int TypedAsyncCommandTestMethodInvokeCount { get; private set; }

        public void TypedMetadataAndCommandTestMethod(CommandMetadata metadata, Command command)
        {
            TypedMetadataAndCommandTestMethodInvokeCount++;
        }
        public void TypedCommandAndMetadataTestMethod(Command command, CommandMetadata metadata)
        {
            TypedCommandAndMetadataTestMethodInvokeCount++;
        }
        public void TypedCommandTestMethod(Command command)
        {
            TypedCommandTestMethodInvokeCount++;
        }
        public Task TypedAsyncMetadataAndCommandTestMethod(CommandMetadata metadata, Command command)
        {
            TypedAsyncMetadataAndCommandTestMethodInvokeCount++;
            return Task.CompletedTask;
        }
        public Task TypedAsyncCommandAndMetadataTestMethod(Command command, CommandMetadata metadata)
        {
            TypedAsyncCommandAndMetadataTestMethodInvokeCount++;
            return Task.CompletedTask;
        }
        public Task TypedAsyncCommandTestMethod(Command command)
        {
            TypedAsyncCommandTestMethodInvokeCount++;
            return Task.CompletedTask;
        }
        public void ThrowsExceptionMethod(Command command)
        {
            throw new NotSupportedException();
        }
    }
}
