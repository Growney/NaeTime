using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Core.ASPNET.Rewrite;
using EventSourcingCore.Readiness.Core;

namespace EventSourcingCore.Readiness.ASPNET
{
    public class RedirectWhenApplicationNotReady : IAsyncRule
    {
        private readonly string _path;
        public RedirectWhenApplicationNotReady(string path)
        {
            _path = path.ToLower();
        }
        public async Task ApplyRule(RewriteContext context)
        {
            if (context.HttpContext.Request.Path.ToString().ToLower() == _path)
            {
                context.Result = RuleResult.SkipRemainingRules;
                return;
            }

            var services = context.HttpContext.RequestServices;

            var readyResults = services.GetReadinessResults();

            bool ready = true;
            foreach (var result in readyResults)
            {
                var r = await result;
                if (!r.IsReady())
                {
                    ready = false;
                    break;
                }
            }

            if (!ready)
            {
                var response = context.HttpContext.Response;
                response.StatusCode = StatusCodes.Status307TemporaryRedirect;
                context.Result = RuleResult.EndResponse;
                response.Headers[HeaderNames.Location] = $"{context.HttpContext.Request.PathBase}{_path}";
            }
        }
    }
}
