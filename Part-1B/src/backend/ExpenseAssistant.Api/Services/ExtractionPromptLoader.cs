using System.Text.RegularExpressions;

namespace ExpenseAssistant.Api.Services;

/// <summary>
/// Loads the system instruction and response schema straight out of
/// src/prompts/extraction-prompt.md (the single source of truth for both), so the prompt
/// documented for evidence purposes is the exact prompt actually sent to Gemini — never a
/// second, silently-drifting copy duplicated in code.
/// </summary>
public static partial class ExtractionPromptLoader
{
    public static (string SystemInstruction, string ResponseSchemaJson) Load(string contentRootPath)
    {
        var promptPath = Path.Combine(contentRootPath, "..", "..", "prompts", "extraction-prompt.md");
        var markdown = File.ReadAllText(promptPath);

        var systemInstruction = ExtractFencedBlock(markdown, "## System Instruction", "text");
        var responseSchemaJson = ExtractFencedBlock(markdown, "## Response Schema", "json");

        return (systemInstruction, responseSchemaJson);
    }

    private static string ExtractFencedBlock(string markdown, string headingMarker, string fenceLanguage)
    {
        var headingIndex = markdown.IndexOf(headingMarker, StringComparison.Ordinal);
        if (headingIndex < 0)
        {
            throw new InvalidOperationException($"extraction-prompt.md is missing the '{headingMarker}' section.");
        }

        var afterHeading = markdown[headingIndex..];
        var match = FencedBlockRegex(fenceLanguage).Match(afterHeading);
        if (!match.Success)
        {
            throw new InvalidOperationException(
                $"extraction-prompt.md's '{headingMarker}' section has no ```{fenceLanguage} code block.");
        }

        return match.Groups[1].Value.Trim();
    }

    [GeneratedRegex(@"```(?:text|json)\r?\n(.*?)```", RegexOptions.Singleline)]
    private static partial Regex FencedBlockPattern();

    private static Regex FencedBlockRegex(string language) => FencedBlockPattern();
}
