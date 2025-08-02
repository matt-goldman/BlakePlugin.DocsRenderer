using BlakePlugin.DocsRenderer.Types;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BlakePlugin.DocsRenderer.MarkdownExtensions;

public class DocumentSectionRenderer(ILogger? logger = null) : HtmlObjectRenderer<HeadingBlock>
{
    private readonly Stack<(Section section, int level)> _stack = new();
    private readonly List<Section> _sections = [];

    public IReadOnlyList<Section> Sections => _sections;

    protected override void Write(HtmlRenderer renderer, HeadingBlock block)
    {
        logger?.LogDebug("[BlakePlugin.DocsRenderer] Rendering heading: {Level} - {heading}", block.Level, block.Inline?.ToString() ?? "null");

        if (block.Inline == null || block.Inline.Count() == 0)
        {
            logger?.LogDebug("[BlakePlugin.DocsRenderer] Skipping empty heading block.");
            return; // Skip empty headings
        }
                
        var headingText = block.Inline?.FirstChild?.ToString() ?? "";
        var headingId = headingText.ToLowerInvariant().Replace(" ", "-");

        var level = block.Level;
        var newSection = new Section { Id = headingId, Text = headingText };

        while (_stack.Count > 0 && _stack.Peek().level >= level)
        {
            logger?.LogDebug("[BlakePlugin.DocsRenderer] Closing section: {sectionId} at level {level}", _stack.Peek().section.Id, _stack.Peek().level);
            _stack.Pop();
            renderer.WriteLine("</section>");
        }

        if (_stack.Count > 0)
        {
            _stack.Peek().section.Children.Add(newSection);
        }
        else
        {
            _sections.Add(newSection);
        }

        _stack.Push((newSection, level));

        logger?.LogDebug("[BlakePlugin.DocsRenderer] Opening section: {headingId} at level {level} with text '{headingText}'", headingId, level, headingText);

        renderer.WriteLine($"<section id=\"{headingId}\">");
        renderer.Write($"<h{level}>{headingText}</h{level}>");
    }

    public void CloseRemaining(HtmlRenderer renderer)
    {
        logger?.LogDebug("[BlakePlugin.DocsRenderer] Closing remaining sections.");

        while (_stack.Count > 0)
        {
            renderer.WriteLine("</section>");
            _stack.Pop();
        }

        logger?.LogDebug("[BlakePlugin.DocsRenderer] All sections closed.");

        var sectionJson = System.Text.Json.JsonSerializer.Serialize(_sections);
        renderer.WriteLine($"<!-- blake:sections:{sectionJson} -->");

        logger?.LogDebug("[BlakePlugin.DocsRenderer] Sections JSON written to renderer."); 
    }
}

