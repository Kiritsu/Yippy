using Microsoft.AspNetCore.Builder;
using Serilog;

namespace Yippy.Common;

public static class SerilogExtensions
{
    public static void UseYippySerilogRequestLogging(this WebApplication @this)
    {
        @this.UseWhen(
            ctx => ctx.Request.Method != HttpMethod.Options.Method, 
            localBuilder =>
            {
                localBuilder.UseSerilogRequestLogging(x =>
                {
                    x.IncludeQueryInRequestPath = true;
                    x.EnrichDiagnosticContext = (context, httpContext) =>
                    {
                        var isPresent = httpContext.Request.Headers.TryGetValue("X-Yippy-Trace", out var traceId);
                        context.Set("YippyTrace", isPresent ? traceId.ToString() : "Unknown");
                    };
                    x.MessageTemplate =
                        "[{YippyTrace}] HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                });
            });
    }
}