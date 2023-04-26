using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ASPNET.Rewrite
{
    public class AsyncRewriteOptions
    {
        public IList<IAsyncRule> Rules { get; } = new List<IAsyncRule>();
        public IFileProvider StaticFileProvider { get; set; }
    }
}
