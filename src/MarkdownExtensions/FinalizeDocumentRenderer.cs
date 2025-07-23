using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace BlakePlugin.DocsRenderer.MarkdownExtensions;

public class FinalizingDocumentRenderer(DocumentSectionRenderer sectionRenderer) : HtmlObjectRenderer<MarkdownDocument>
{
    protected override void Write(HtmlRenderer renderer, MarkdownDocument document)
    {
        foreach (var block in document)
        {
            renderer.Write(block); // delegate to existing block renderers
        }

        sectionRenderer.CloseRemaining(renderer);
    }
}
