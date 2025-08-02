using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Microsoft.Extensions.Logging;

namespace BlakePlugin.DocsRenderer.MarkdownExtensions;

public class FinalizingDocumentRenderer(DocumentSectionRenderer sectionRenderer, ILogger? logger = null) : HtmlObjectRenderer<MarkdownDocument>
{
    protected override void Write(HtmlRenderer renderer, MarkdownDocument document)
    {
        logger?.LogDebug("[BlakePlugin.DocsRenderer] Finalizing document rendering.");

        foreach (var block in document)
        {
            logger?.LogDebug("[BlakePlugin.DocsRenderer] Processing block: {Name}", block.GetType().Name);

            renderer.Write(block); // delegate to existing block renderers
        }

        sectionRenderer.CloseRemaining(renderer);
    }
}
