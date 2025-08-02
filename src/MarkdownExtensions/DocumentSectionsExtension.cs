using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Microsoft.Extensions.Logging;

namespace BlakePlugin.DocsRenderer.MarkdownExtensions;

public class DocumentSectionsExtension(ILogger? logger = null) : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline) { }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        logger?.LogDebug("[BlakePlugin.DocsRenderer] Setting up DocumentSectionsExtension.");

        if (renderer is HtmlRenderer htmlRenderer)
        {
            // Remove any existing renderers for HeadingBlock
            var toRemove = htmlRenderer.ObjectRenderers
                .Where(r => r is HtmlObjectRenderer<HeadingBlock>)
                .ToList();

            foreach (var r in toRemove)
                htmlRenderer.ObjectRenderers.Remove(r);

            // Now insert yours at the top
            var sectionRenderer = new DocumentSectionRenderer(logger);
            htmlRenderer.ObjectRenderers.Insert(0, sectionRenderer);


            htmlRenderer.ObjectRenderers.ReplaceOrAdd<FinalizingDocumentRenderer>(
                new FinalizingDocumentRenderer(sectionRenderer, logger)
            );
        }
    }
}
