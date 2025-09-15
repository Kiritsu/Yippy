using System.Text;
using System.Text.RegularExpressions;

namespace Yippy.Templating.Services;

public interface ITemplateVariableProcessor
{
    (string processedContent, Dictionary<string, string> unmappedVariables) ProcessTemplate(
        string templateContent, 
        Dictionary<string, string> providedVariables);
}

public partial class TemplateVariableProcessor : ITemplateVariableProcessor
{
    private static readonly Regex VariablePattern = VariablePatternRegex();

    public (string processedContent, Dictionary<string, string> unmappedVariables) ProcessTemplate(
        string templateContent, 
        Dictionary<string, string> providedVariables)
    {
        if (string.IsNullOrEmpty(templateContent))
        {
            return (string.Empty, []);
        }

        var unmappedVariables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var processedContent = new StringBuilder(templateContent);
        
        var matches = VariablePattern.Matches(templateContent);
        var replacements = new List<(int start, int length, string value)>();
        
        foreach (Match match in matches)
        {
            var variableName = match.Groups[1].Value;
            var fullMatch = match.Value;
            
            if (providedVariables.TryGetValue(variableName, out var value))
            {
                replacements.Add((match.Index, match.Length, value));
            }
            else
            {
                unmappedVariables[variableName] = fullMatch;
            }
        }
        
        foreach (var (start, length, value) in replacements.OrderByDescending(r => r.start))
        {
            processedContent.Remove(start, length);
            processedContent.Insert(start, value);
        }
        
        return (processedContent.ToString(), unmappedVariables);
    }

    [GeneratedRegex(@"\{\{\s*(\w+)\s*\}\}", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex VariablePatternRegex();
}