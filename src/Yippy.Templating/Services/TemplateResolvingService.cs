using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Yippy.Templating.Data;

namespace Yippy.Templating.Services;

public class TemplateResolvingService(
    YippyTemplatingDbContext dbContext,
    ITemplateVariableProcessor variableProcessor,
    IMemoryCache cache,
    ILogger<TemplateResolvingService> logger)
    : TemplateResolver.TemplateResolverBase
{
    private const string EmailTemplateCacheKeyPrefix = "email_template_";
    private const string SmsTemplateCacheKeyPrefix = "sms_template_";
    
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    public override async Task<ResolveEmailTemplateResponse> GetEmail(
        ResolveEmailTemplateRequest request, 
        ServerCallContext context)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.TemplateName))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, 
                    "Template name is required"));
            }
            
            var cacheKey = $"{EmailTemplateCacheKeyPrefix}{request.TemplateName.ToLowerInvariant()}";
            var template = await cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                entry.Priority = CacheItemPriority.Normal;
                
                return await dbContext.EmailTemplates
                    .AsNoTracking()
                    .Where(t => t.Name == request.TemplateName)
                    .FirstOrDefaultAsync(context.CancellationToken);
            });
            
            if (template == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, 
                    $"Email template '{request.TemplateName}' not found"));
            }
            
            var variables = request.Variables?.ToDictionary(
                kvp => kvp.Key, 
                kvp => kvp.Value) ?? new Dictionary<string, string>();
            
            var (processedSubject, unmappedInSubject) = variableProcessor.ProcessTemplate(
                template.Subject, variables);
            
            var (processedBody, unmappedInBody) = variableProcessor.ProcessTemplate(
                template.Body, variables);
            
            var allUnmapped = unmappedInSubject
                .Concat(unmappedInBody)
                .DistinctBy(kvp => kvp.Key)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var response = new ResolveEmailTemplateResponse
            {
                FromName = template.FromName,
                FromEmail = template.FromEmail,
                Object = processedSubject,
                RawBody = processedBody
            };
            
            foreach (var unmapped in allUnmapped)
            {
                response.UnmappedVariables[unmapped.Key] = unmapped.Value;
            }

            return response;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.ErrorResolvingEmailTemplate(ex, request.TemplateName);
            throw new RpcException(new Status(StatusCode.Internal, 
                "An error occurred while processing the email template"));
        }
    }
    
    public override async Task<ResolveSmsTemplateResponse> GetSms(
        ResolveSmsTemplateRequest request, 
        ServerCallContext context)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.TemplateName))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, 
                    "Template name is required"));
            }
            
            var cacheKey = $"{SmsTemplateCacheKeyPrefix}{request.TemplateName.ToLowerInvariant()}";
            var template = await cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                entry.Priority = CacheItemPriority.Normal;
                
                return await dbContext.SmsTemplates
                    .AsNoTracking()
                    .Where(t => t.Name == request.TemplateName)
                    .FirstOrDefaultAsync(context.CancellationToken);
            });
            
            if (template == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, 
                    $"SMS template '{request.TemplateName}' not found"));
            }
            
            var variables = request.Variables?.ToDictionary(
                kvp => kvp.Key, 
                kvp => kvp.Value) ?? new Dictionary<string, string>();
            
            var (processedBody, unmappedInBody) = variableProcessor.ProcessTemplate(
                template.Body, variables);

            var response = new ResolveSmsTemplateResponse
            {
                FromName = template.FromName,
                RawBody = processedBody
            };
            
            foreach (var unmapped in unmappedInBody)
            {
                response.UnmappedVariables[unmapped.Key] = unmapped.Value;
            }
            
            return response;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.ErrorResolvingSmsTemplate(ex, request.TemplateName);
            throw new RpcException(new Status(StatusCode.Internal, 
                "An error occurred while processing the SMS template"));
        }
    }
}