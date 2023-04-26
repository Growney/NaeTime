using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Serialization.Json.Tests.Implementations
{
    class ImmutableParentObject
    {
        public ImmutableParentObject(ImmutableBuiltInTypeObject childOne, ImmutableBuiltInTypeObject childTwo)
        {
            ChildOne = childOne;
            ChildTwo = childTwo;
        }

        public ImmutableBuiltInTypeObject ChildOne { get; }
        public ImmutableBuiltInTypeObject ChildTwo { get; }
    }
}
