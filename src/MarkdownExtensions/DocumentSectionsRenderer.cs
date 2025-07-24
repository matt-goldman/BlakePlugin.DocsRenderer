using BlakePlugin.DocsRenderer.Types;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using System.Diagnostics;

namespace BlakePlugin.DocsRenderer.MarkdownExtensions;

public class DocumentSectionRenderer : HtmlObjectRenderer<HeadingBlock>
{
    private readonly Stack<(Section section, int level)> _stack = new();
    private readonly List<Section> _sections = [];

    public IReadOnlyList<Section> Sections => _sections;

    protected override void Write(HtmlRenderer renderer, HeadingBlock block)
    {
        Console.WriteLine($"[BlakePlugin.DocsRenderer] Rendering heading: {block.Level} - {block.Inline?.ToString() ?? "null"}");
        if (block.Inline == null || block.Inline.Count() == 0)
        {
            Console.WriteLine("[BlakePlugin.DocsRenderer] Skipping empty heading block.");
            return; // Skip empty headings
        }

        Debug.Assert(block.Inline != null, "HeadingBlock should have Inline content.");
        
        var headingText = block.Inline?.FirstChild?.ToString() ?? "";
        var headingId = headingText.ToLowerInvariant().Replace(" ", "-");

        var level = block.Level;
        var newSection = new Section { Id = headingId, Text = headingText };

        while (_stack.Count > 0 && _stack.Peek().level >= level)
        {
            Console.WriteLine($"[BlakePlugin.DocsRenderer] Closing section: {_stack.Peek().section.Id} at level {_stack.Peek().level}");
            _stack.Pop();
            renderer.WriteLine("</Section>");
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

        Console.WriteLine($"[BlakePlugin.DocsRenderer] Opening section: {headingId} at level {level} with text '{headingText}'");

        renderer.WriteLine($"<Section id=\"{headingId}\">");
        renderer.Write($"<h{level} id=\"{headingId}\">{headingText}</h{level}>");
    }

    public void CloseRemaining(HtmlRenderer renderer)
    {
        Console.WriteLine("[BlakePlugin.DocsRenderer] Closing remaining sections.");

        while (_stack.Count > 0)
        {
            renderer.WriteLine("</Section>");
            _stack.Pop();
        }

        Console.WriteLine("[BlakePlugin.DocsRenderer] All sections closed.");

        var sectionJson = System.Text.Json.JsonSerializer.Serialize(_sections);
        renderer.WriteLine($"<!-- blake:sections:{sectionJson} -->");

        Console.WriteLine("[BlakePlugin.DocsRenderer] Sections JSON written to renderer."); 
    }
}

