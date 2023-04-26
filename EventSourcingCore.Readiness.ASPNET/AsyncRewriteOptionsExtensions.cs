using System;
using System.Collections.Generic;
using System.Text;
using Core.ASPNET.Rewrite;

namespace EventSourcingCore.Readiness.ASPNET
{
    public static class AsyncRewriteOptionsExtensions
    {
        public static AsyncRewriteOptions AddNotReadyRedirect(this AsyncRewriteOptions options, string path)
        {
            return options.Add(new RedirectWhenApplicationNotReady(path));
        }
    }
}
