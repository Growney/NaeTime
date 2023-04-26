using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Core.Reflection.Abstractions;
using Xunit;

namespace Core.Reflection.Tests
{
    public class StandardDelegateProviderTests
    {
        private readonly IServiceProvider _provider;
        public StandardDelegateProviderTests()
        {
            var collection = new ServiceCollection();
            _provider = collection.BuildServiceProvider();
        }

        public interface IFakeClassWithMethodsWithMultipleNames
        {
            public object When();
            public object On();
            public object Execute();
            public object Execute(string arg1, string arg2);
            public void When(FakeClassThatImplementsNoInterface noImplementation, FakeClassThatImplementsFakeInterface implements);
            public void When(FakeClassThatImplementsFakeInterface implements, FakeClassThatImplementsNoInterface noImplementation);
            public void When(FakeClassThatImplementsFakeInterface implements);
            public void When(FakeClassThatImplementsNoInterface noImplementation);
        }
        public class FakeClassWithOverrideMethod
        {
            [Override]
            public void Whenv2(FakeClassThatImplementsFakeInterface implements)
            {

            }
        }

        public interface IFakeClassWithInterfaceImplementations
        {
            public FakeClassThatImplementsNoInterface When(FakeClassThatImplementsFakeInterface implements, FakeClassThatImplementsSecondFakeInterface implements2);
        }

        public class FakeClassWithTaskMethod
        {
            public Task When(FakeClassThatImplementsFakeInterface implements)
            {
                return Task.CompletedTask;
            }
        }
        public class OverrideAttribute : Attribute
        {

        }
        public interface ISecondFakeInterface
        {

        }
        public interface IFakeInterface
        {

        }
        public class FakeClassThatImplementsFakeInterface : IFakeInterface
        {

        }
        public class FakeClassThatImplementsSecondFakeInterface : ISecondFakeInterface
        {

        }
        public class FakeClassThatImplementsNoInterface
        {

        }
        [Fact]
        public void For_NamedStandard_When_MethodAsksForInterfaces_Expect_CorrectMethodCalls()
        {
            var mock = new Mock<IFakeClassWithInterfaceImplementations>();
            var standard = new MethodStandard(
                    name: "When"
                );
            var provider = new StandardFuncProvider<IFakeInterface, ISecondFakeInterface, FakeClassThatImplementsNoInterface>(_provider, null, standard);

            var methods = provider.GetMethods(mock.Object.GetType());

            var list = methods.ToList();

            var firstClass = new FakeClassThatImplementsFakeInterface();
            var secondClass = new FakeClassThatImplementsSecondFakeInterface();

            list[0].Invoke(mock.Object, firstClass, secondClass);

            mock.Verify(x => x.When(firstClass, secondClass));
        }
        [Fact]
        public void For_NamedStandard_When_ClassHasMethodsWithMultipleNames_Expect_CorrectNumberOfMethods()
        {
            var mock = new Mock<IFakeClassWithMethodsWithMultipleNames>();
            var standard = new MethodStandard(
                    name: "On"
                );
            var provider = new StandardFuncProvider<string, DateTime, object>(_provider, null, standard);

            var methods = provider.GetMethods(typeof(IFakeClassWithMethodsWithMultipleNames));

            var list = methods.ToList();

            Assert.Single(list);
        }
        [Fact]
        public void For_NamedDoNotAllowMissingParametersStandard_When_ClassHasMethodsWithParametersThatAreTheSameType_Expect_CorrectNumberOfMethods()
        {
            var mock = new Mock<IFakeClassWithMethodsWithMultipleNames>();
            var standard = new MethodStandard(
                    name: "Execute",
                    allowMissingParameters: false
                );
            var provider = new StandardFuncProvider<string, string, object>(_provider, null, standard);

            var methods = provider.GetMethods(typeof(IFakeClassWithMethodsWithMultipleNames));

            var list = methods.ToList();

            Assert.Single(list);
        }
        [Fact]
        public void For_NamedDoNotAllowMissingParametersStandard_When_ClassHasMethodsWithParametersThatAreTheSameType_Expect_CorrectMethodCalled()
        {
            var mock = new Mock<IFakeClassWithMethodsWithMultipleNames>();
            var standard = new MethodStandard(
                    name: "Execute",
                    allowMissingParameters: false
                );

            var provider = new StandardFuncProvider<string, string, object>(_provider, null, standard);

            var methods = provider.GetMethods(mock.Object.GetType());

            var list = methods.ToList();

            Assert.Single(list);

            var method = list[0];

            method.Invoke(mock.Object, "arg1", "arg2");
            mock.Verify(x => x.Execute("arg1", "arg2"));
            mock.VerifyNoOtherCalls();
        }

        [Fact]
        public void For_NamedStandard_When_ClassMethodExpectInterfaceAndClass_Expect_CorrectMethodCalled()
        {
            var mock = new Mock<IFakeClassWithMethodsWithMultipleNames>();
            var standard = new MethodStandard(
                    name: "When"
                );

            var provider = new StandardActionProvider<IFakeInterface, FakeClassThatImplementsNoInterface>(_provider, null, standard);

            var methods = provider.GetMethods(mock.Object.GetType());

            foreach (var method in methods)
            {
                method.Invoke(mock.Object, new FakeClassThatImplementsFakeInterface(), new FakeClassThatImplementsNoInterface());
            }

            mock.Verify(x => x.When(It.IsAny<FakeClassThatImplementsNoInterface>(), It.IsAny<FakeClassThatImplementsFakeInterface>()));
            mock.Verify(x => x.When(It.IsAny<FakeClassThatImplementsFakeInterface>(), It.IsAny<FakeClassThatImplementsNoInterface>()));
            mock.Verify(x => x.When(It.IsAny<FakeClassThatImplementsFakeInterface>()));
            mock.Verify(x => x.When(It.IsAny<FakeClassThatImplementsNoInterface>()));
            mock.Verify(x => x.When());
            mock.VerifyNoOtherCalls();
        }

        [Fact]
        public void For_NamedStandard_When_ClassMethodExpectInterfaceAndClassInterfaceImplementationIsRequired_Expect_CorrectMethodCalled()
        {
            var mock = new Mock<IFakeClassWithMethodsWithMultipleNames>();
            var standard = new MethodStandard(
                    name: "When"
                );

            var provider = new StandardActionProvider<IFakeInterface, FakeClassThatImplementsNoInterface>(_provider, null, standard);

            var methods = provider.GetMethods(mock.Object.GetType(), t1Required: true);

            foreach (var method in methods)
            {
                method.Invoke(mock.Object, new FakeClassThatImplementsFakeInterface(), new FakeClassThatImplementsNoInterface());
            }

            mock.Verify(x => x.When(It.IsAny<FakeClassThatImplementsNoInterface>(), It.IsAny<FakeClassThatImplementsFakeInterface>()));
            mock.Verify(x => x.When(It.IsAny<FakeClassThatImplementsFakeInterface>(), It.IsAny<FakeClassThatImplementsNoInterface>()));
            mock.Verify(x => x.When(It.IsAny<FakeClassThatImplementsFakeInterface>()));
            mock.VerifyNoOtherCalls();
        }
        [Fact]
        public void For_NamedStandardOverrideAttribute_When_ClassMethodHasDifferentNameButOverrideAttibute_Expect_MethodMeetsStandard()
        {
            var standard = new MethodStandard(
                    name: "When",
                    overrideAttributes: new Type[] { typeof(OverrideAttribute) }
                );

            var provider = new StandardActionProvider<IFakeInterface, FakeClassThatImplementsNoInterface>(_provider, null, standard);

            var methods = provider.GetMethods(typeof(FakeClassWithOverrideMethod), t1Required: true);

            Assert.Single(methods);

        }

        [Fact]
        public void For_NamedStandardOverrideAttribute_When_ClassMethodReturnsTaskAndGetAsync_Expect_MethodMeetsStandard()
        {
            var standard = new MethodStandard(
                    name: "When"
                );

            var provider = new StandardActionProvider<IFakeInterface, FakeClassThatImplementsNoInterface>(_provider, null, standard);

            var methods = provider.GetAsyncMethods(typeof(FakeClassWithTaskMethod), t1Required: true);

            Assert.Single(methods);

        }
    }
}
