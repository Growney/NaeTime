using Microsoft.AspNetCore.Rewrite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.ASPNET.Rewrite
{
    public interface IAsyncRule
    {
        Task ApplyRule(RewriteContext context);
    }
}
