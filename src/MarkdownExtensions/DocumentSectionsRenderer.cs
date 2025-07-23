using BlakePlugin.DocsRenderer.Types;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace BlakePlugin.DocsRenderer.MarkdownExtensions;

public class DocumentSectionRenderer : HtmlObjectRenderer<HeadingBlock>
{
    private readonly Stack<(Section section, int level)> _stack = new();
    private readonly List<Section> _sections = [];

    public IReadOnlyList<Section> Sections => _sections;

    protected override void Write(HtmlRenderer renderer, HeadingBlock block)
    {
        var headingText = block.Inline?.FirstChild?.ToString() ?? "";
        var headingId = headingText.ToLowerInvariant().Replace(" ", "-");

        var level = block.Level;
        var newSection = new Section { Id = headingId, Text = headingText };

        while (_stack.Count > 0 && _stack.Peek().level >= level)
        {
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

        renderer.WriteLine($"<Section id=\"{headingId}\">");
        renderer.Write($"<h{level} id=\"{headingId}\">{headingText}</h{level}>");
    }

    public void CloseRemaining(HtmlRenderer renderer)
    {
        while (_stack.Count > 0)
        {
            renderer.WriteLine("</Section>");
            _stack.Pop();
        }

        var sectionJson = System.Text.Json.JsonSerializer.Serialize(_sections);
        renderer.WriteLine($"<!-- blake:sections:{sectionJson} -->");
    }
}

