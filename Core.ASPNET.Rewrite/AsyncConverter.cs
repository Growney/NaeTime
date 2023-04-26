using Microsoft.AspNetCore.Rewrite;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Core.ASPNET.Rewrite
{
    public class AsyncConverter : IAsyncRule
    {
        private readonly IRule _rule;
        public AsyncConverter(IRule rule)
        {
            _rule = rule;
        }

        public Task ApplyRule(RewriteContext context) =>
            Task.Run(() => _rule.ApplyRule(context));
    }
}
