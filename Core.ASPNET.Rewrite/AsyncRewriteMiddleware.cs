using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.ASPNET.Rewrite
{
    public class AsyncRewriteMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AsyncRewriteOptions _options;
        private readonly IFileProvider _fileProvider;
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of <see cref="RewriteMiddleware"/>
        /// </summary>
        /// <param name="next">The delegate representing the next middleware in the request pipeline.</param>
        /// <param name="hostingEnvironment">The Hosting Environment.</param>
        /// <param name="loggerFactory">The Logger Factory.</param>
        /// <param name="options">The middleware options, containing the rules to apply.</param>
        public AsyncRewriteMiddleware(
            RequestDelegate next,
            ILoggerFactory loggerFactory,
            IOptions<AsyncRewriteOptions> options)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _next = next;
            _options = options.Value;
            _fileProvider = _options.StaticFileProvider;
            _logger = loggerFactory.CreateLogger<RewriteMiddleware>();
        }

        /// <summary>
        /// Executes the middleware.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
        /// <returns>A task that represents the execution of this middleware.</returns>
        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var rewriteContext = new RewriteContext
            {
                HttpContext = context,
                StaticFileProvider = _fileProvider,
                Logger = _logger,
                Result = RuleResult.ContinueRules
            };

            foreach (var rule in _options.Rules)
            {
                await rule.ApplyRule(rewriteContext);
                switch (rewriteContext.Result)
                {
                    case RuleResult.ContinueRules:
                        continue;
                    case RuleResult.EndResponse:
                        return;
                    case RuleResult.SkipRemainingRules:
                        await _next(context);
                        return;
                    default:
                        throw new ArgumentOutOfRangeException($"Invalid rule termination {rewriteContext.Result}");
                }
            }
            await _next(context);
        }
    }
}
