using Blake.BuildTools;
using BlakePlugin.DocsRenderer.MarkdownExtensions;
using BlakePlugin.DocsRenderer.Types;
using Microsoft.Extensions.Logging;
using System.Text;

namespace BlakePlugin.DocsRenderer;

public class Plugin : IBlakePlugin
{
    private static readonly PrismOptions _options = new()
    {
        UseLineNumbers      = true,
        UseCopyButton       = true,
        UseLineHighlighting = true,
        UseLineDiff         = true
    };

    public Task BeforeBakeAsync(BlakeContext context, ILogger? logger = null)
    {
        // This method is called before the bake process starts
        // You can add any pre-bake logic here if needed

        logger?.LogInformation("[BlakePlugin.DocsRenderer] BeforeBakeAsync called.");

        context.PipelineBuilder
            .UsePrism(_options, logger)
            .UseDocumentSections(logger);

        return Task.CompletedTask;
    }

    public Task AfterBakeAsync(BlakeContext context, ILogger? logger = null)
    {
        // This method is called after the bake process completes
        logger?.LogInformation("[BlakePlugin.DocsRenderer] AfterBakeAsync called.");

        foreach (var page in context.GeneratedPages.ToList())
        {
            if (string.IsNullOrWhiteSpace(page.RazorHtml))
                continue;

            logger?.LogInformation("Processing page: {Title} ({Slug})", page.Page.Title, page.Page.Slug);

            if (page.RazorHtml.Contains("<!-- blake:sections:"))
            {
                logger?.LogInformation("Found sections in page: {Title} ({Slug})", page.Page.Title, page.Page.Slug);

                var sectionsJson = page.RazorHtml
                    .Split("<!-- blake:sections:")[1]
                    .Split("-->")[0]
                    .Trim();

                var sections = System.Text.Json.JsonSerializer.Deserialize<List<Section>>(sectionsJson);

                if (sections == null || sections.Count == 0)
                {
                    logger?.LogInformation("No sections found in page: {Title} ({Slug})", page.Page.Title, page.Page.Slug);
                    continue; // No sections found, skip processing
                }

                logger?.LogInformation("Found {Count} sections in page: {Title} ({Slug})", sections.Count, page.Page.Title, page.Page.Slug);

                var staticSectionList = GetStaticSectionList(sections);

                var updatedHtml = page.RazorHtml
                    .Replace($"<!-- blake:sections:{sectionsJson} -->", string.Empty)
                    .Trim();

                // add the using statement to the top of the RazorHtml if not already present
                if (!updatedHtml.Contains("using BlakePlugin.DocsRenderer.Types;"))
                {
                    logger?.LogInformation("Adding using statement for BlakePlugin.DocsRenderer.Types.");
                    updatedHtml = $"@using BlakePlugin.DocsRenderer.Types;{Environment.NewLine}{updatedHtml}";
                }
                else
                {
                    logger?.LogInformation("Using statement for BlakePlugin.DocsRenderer.Types already present.");
                }

                if (updatedHtml.Contains("@code"))
                {
                    logger?.LogInformation("Found existing @code block in RazorHtml.");
                    // Find the last closing brace of the @code block
                    var lastBraceIndex = updatedHtml.LastIndexOf('}');

                    if (lastBraceIndex >= 0)
                    {
                        // Insert the static section list before the last closing brace
                        updatedHtml = updatedHtml.Insert(lastBraceIndex, $"{Environment.NewLine}{staticSectionList}");
                    }
                    else
                    {
                        // malformed RazorHtml, skip and log an error
                        logger?.LogWarning("Could not find a valid @code block in the RazorHtml in page {Title}.", page.Page.Title);
                    }
                }
                else
                {
                    logger?.LogInformation("No existing @code block found in RazorHtml.");
                    // If no @code block found, append the static section list at the end
                    updatedHtml += $"{Environment.NewLine}@code{Environment.NewLine}{{{Environment.NewLine}{staticSectionList}{Environment.NewLine}}}";
                }

                var updatedPage = new GeneratedPage(page.Page, page.OutputPath, updatedHtml);
                context.GeneratedPages[context.GeneratedPages.IndexOf(page)] = updatedPage;
            }
        }

        return Task.CompletedTask;
    }

    private static string GetStaticSectionList(IEnumerable<Section> sections)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"    private List<Section> _sections = [");
        foreach (var section in sections)
        {
            AppendSection(section, sb, 2);
        }
        sb.AppendLine("    ];");
        
        return sb.ToString();
    }

    private static void AppendSection(Section section, StringBuilder sb, int indentLevel = 0)
    {
        var indent = new string(' ', indentLevel * 4);

        sb.AppendLine($$"""
    {{indent}}new Section {
    {{indent}}    Id = @"{{EscapeVerbatim(section.Id)}}",
    {{indent}}    Text = @"{{EscapeVerbatim(section.Text)}}",
    {{indent}}    Children = [
    """);

        foreach (var child in section.Children)
        {
            AppendSection(child, sb, indentLevel + 1);
        }

        sb.AppendLine($"{indent}] }},");
    }


    private static string EscapeVerbatim(string input) =>
    input.Replace("\"", "\"\"");

}
