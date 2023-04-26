using Microsoft.AspNetCore.Rewrite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ASPNET.Rewrite
{
    public static class AsyncRewriteOptionsExtensionMethods
    {
        public static AsyncRewriteOptions Add(this AsyncRewriteOptions options, IAsyncRule rule)
        {
            options.Rules.Add(rule);

            return options;
        }
        public static AsyncRewriteOptions Add(this AsyncRewriteOptions options, IRule rule)
        {
            options.Rules.Add(new AsyncConverter(rule));

            return options;
        }
    }
}
