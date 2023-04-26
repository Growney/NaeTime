using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Core.Security.Abstractions;

namespace EventSourcingCore.Persistence.EF.Abstractions
{
    public class CustomerIDGenerator : ValueGenerator<Guid>
    {
        public override bool GeneratesTemporaryValues => false;

        public override Guid Next([NotNull] EntityEntry entry)
        {
            var accessor = entry.Context.GetService<ICustomerContextAccessor>();

            return accessor.Context.CustomerID;
        }
    }
}
