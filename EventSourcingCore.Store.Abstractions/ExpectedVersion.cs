using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingCore.Store.Abstractions
{
    public static class ExpectedVersion
    {
        //
        // Summary:
        //     The write should not conflict with anything and should always succeed.
        public const int Any = -2;
        //
        // Summary:
        //     The stream should not yet exist. If it does exist treat that as a concurrency
        //     problem.
        public const int NoStream = -1;
        //
        // Summary:
        //     The stream should exist. If it or a metadata stream does not exist treat that
        //     as a concurrency problem.
        public const int StreamExists = -3;
    }
}
