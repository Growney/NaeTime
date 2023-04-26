using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Serialization.Json.Tests.Implementations
{
    public class ImmutableBuiltInTypeObject
    {
        public ImmutableBuiltInTypeObject(bool boolVal, byte byteVal, char charVal, decimal decimalVal, double doubleVal, float floatVal, int intVal, uint uIntVal, long longVal, ulong uLongVal, short shortVal, ushort uShortVal, string stringVal)
        {
            BoolVal = boolVal;
            ByteVal = byteVal;
            CharVal = charVal;
            DecimalVal = decimalVal;
            DoubleVal = doubleVal;
            FloatVal = floatVal;
            IntVal = intVal;
            UIntVal = uIntVal;
            LongVal = longVal;
            ULongVal = uLongVal;
            ShortVal = shortVal;
            UShortVal = uShortVal;
            StringVal = stringVal;
        }

        public bool BoolVal { get; }
        public byte ByteVal { get; }
        public char CharVal { get; }
        public decimal DecimalVal { get; }
        public double DoubleVal { get; }
        public float FloatVal { get; }
        public int IntVal { get; }
        public uint UIntVal { get; }
        public long LongVal { get; }
        public ulong ULongVal { get; }
        public short ShortVal { get; }
        public ushort UShortVal { get; }
        public string StringVal { get; }
    }
}
