using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace BlakePlugin.DocsRenderer.MarkdownExtensions;

public class FinalizingDocumentRenderer : HtmlObjectRenderer<MarkdownDocument>
{
    private readonly DocumentSectionRenderer _sectionRenderer;

    public FinalizingDocumentRenderer(DocumentSectionRenderer sectionRenderer)
    {
        _sectionRenderer = sectionRenderer;
    }

    protected override void Write(HtmlRenderer renderer, MarkdownDocument document)
    {
        foreach (var block in document)
        {
            renderer.Write(block); // delegate to existing block renderers
        }

        _sectionRenderer.CloseRemaining(renderer);
    }
}
