using Blake.BuildTools;
using BlakePlugin.DocsRenderer.MarkdownExtensions;
using BlakePlugin.DocsRenderer.Types;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Web;

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

            var updatedHtml = page.RazorHtml;
            var hasChanges = false;

            // Process sections
            List<Section> sections = [];
            if (updatedHtml.Contains("<!-- blake:sections:"))
            {
                logger?.LogInformation("Found sections in page: {Title} ({Slug})", page.Page.Title, page.Page.Slug);

                var sectionsJson = updatedHtml
                    .Split("<!-- blake:sections:")[1]
                    .Split("-->")[0]
                    .Trim();

                sections = System.Text.Json.JsonSerializer.Deserialize<List<Section>>(sectionsJson) ?? [];

                if (sections.Count == 0)
                {
                    logger?.LogInformation("No sections found in page: {Title} ({Slug}).", page.Page.Title, page.Page.Slug);
                }
                logger?.LogInformation("Found {Count} sections in page: {Title} ({Slug})", sections.Count, page.Page.Title, page.Page.Slug);

                updatedHtml = updatedHtml.Replace($"<!-- blake:sections:{sectionsJson} -->", string.Empty).Trim();
                hasChanges = true;
            }

            // Process code blocks
            var codeBlocks = ExtractCodeBlocks(updatedHtml, logger);
            if (codeBlocks.Count > 0)
            {
                logger?.LogInformation("Found {Count} Razor code blocks in page: {Title} ({Slug})", codeBlocks.Count, page.Page.Title, page.Page.Slug);
                
                // Remove code block comments from HTML
                foreach (var (comment, _, _) in codeBlocks)
                {
                    updatedHtml = updatedHtml.Replace(comment, string.Empty);
                }
                hasChanges = true;
            }

            // Update the page if there were any changes or if we need to add empty sections
            if (hasChanges || sections.Count == 0)
            {
                updatedHtml = AddSectionsAndCodeBlocksToPage(updatedHtml, sections, codeBlocks, page.Page.Title, logger);

                var updatedPage = new GeneratedPage(page.Page, page.OutputPath, updatedHtml, page.RawHtml);
                context.GeneratedPages[context.GeneratedPages.IndexOf(page)] = updatedPage;
            }
        }

        return Task.CompletedTask;
    }

    private static List<(string comment, string variableName, string codeContent)> ExtractCodeBlocks(string html, ILogger? logger)
    {
        var codeBlocks = new List<(string comment, string variableName, string codeContent)>();
        var codeBlockMarker = "<!-- blake:codeblock:";
        
        var startIndex = 0;
        while (true)
        {
            var commentStart = html.IndexOf(codeBlockMarker, startIndex);
            if (commentStart == -1) break;
            
            var commentEnd = html.IndexOf("-->", commentStart);
            if (commentEnd == -1)
            {
                logger?.LogWarning("Found malformed code block comment (no end marker)");
                break;
            }
            
            var fullComment = html.Substring(commentStart, commentEnd + 3 - commentStart);
            var commentContent = html.Substring(commentStart + codeBlockMarker.Length, commentEnd - commentStart - codeBlockMarker.Length);
            
            var parts = commentContent.Split(':');
            if (parts.Length == 2)
            {
                var variableName = parts[0];
                var encodedContent = parts[1];
                
                try
                {
                    var decodedBytes = Convert.FromBase64String(encodedContent);
                    var codeContent = System.Text.Encoding.UTF8.GetString(decodedBytes);
                    codeBlocks.Add((fullComment, variableName, codeContent));
                }
                catch (Exception ex)
                {
                    logger?.LogWarning("Failed to decode code block content: {Error}", ex.Message);
                }
            }
            else
            {
                logger?.LogWarning("Found malformed code block comment (invalid format)");
            }
            
            startIndex = commentEnd + 3;
        }
        
        return codeBlocks;
    }

    private static string AddSectionsAndCodeBlocksToPage(string razorHtml, List<Section> sections, List<(string comment, string variableName, string codeContent)> codeBlocks, string pageTitle, ILogger? logger)
    {
        var updatedHtml = razorHtml;

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

        // Generate the content to add to the @code block
        var codeBlockContent = new StringBuilder();
        
        // Add sections
        var staticSectionList = GetStaticSectionList(sections);
        codeBlockContent.AppendLine(staticSectionList);
        
        // Add code block variables
        if (codeBlocks.Count > 0)
        {
            codeBlockContent.AppendLine();
            codeBlockContent.AppendLine("    // Razor code block variables to prevent @ symbol issues");
            foreach (var (_, variableName, codeContent) in codeBlocks)
            {
                // HTML-encode the code content so it displays as text instead of rendering HTML tags
                var htmlEncodedContent = HttpUtility.HtmlEncode(codeContent);
                codeBlockContent.AppendLine($"    private string {variableName} = @\"{EscapeVerbatim(htmlEncodedContent)}\";");
            }
        }

        if (updatedHtml.Contains("@code"))
        {
            logger?.LogInformation("Found existing @code block in RazorHtml.");
            // Find the last closing brace of the @code block
            var lastBraceIndex = updatedHtml.LastIndexOf('}');

            if (lastBraceIndex >= 0)
            {
                // Insert the content before the last closing brace
                updatedHtml = updatedHtml.Insert(lastBraceIndex, $"{Environment.NewLine}{codeBlockContent}");
            }
            else
            {
                // malformed RazorHtml, skip and log an error
                logger?.LogWarning("Could not find a valid @code block in the RazorHtml in page {Title}.", pageTitle);
            }
        }
        else
        {
            logger?.LogInformation("No existing @code block found in RazorHtml.");
            // If no @code block found, append the content at the end
            updatedHtml += $"{Environment.NewLine}@code{Environment.NewLine}{{{Environment.NewLine}{codeBlockContent}{Environment.NewLine}}}";
        }

        return updatedHtml;
    }

    private static string AddSectionsToPage(string razorHtml, string staticSectionList, string pageTitle, ILogger? logger)
    {
        var updatedHtml = razorHtml;

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
                logger?.LogWarning("Could not find a valid @code block in the RazorHtml in page {Title}.", pageTitle);
            }
        }
        else
        {
            logger?.LogInformation("No existing @code block found in RazorHtml.");
            // If no @code block found, append the static section list at the end
            updatedHtml += $"{Environment.NewLine}@code{Environment.NewLine}{{{Environment.NewLine}{staticSectionList}{Environment.NewLine}}}";
        }

        return updatedHtml;
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
