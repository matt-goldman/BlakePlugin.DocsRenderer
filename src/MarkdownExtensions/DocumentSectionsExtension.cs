using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using System.Diagnostics;

namespace BlakePlugin.DocsRenderer.MarkdownExtensions;

public class DocumentSectionsExtension : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline) { }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        Debug.Assert(renderer is HtmlRenderer, "DocumentSectionsExtension requires an HtmlRenderer.");

        Debug.Assert(pipeline is MarkdownPipeline, "DocumentSectionsExtension requires a MarkdownPipeline.");

        Console.WriteLine("[BlakePlugin.DocsRenderer] Setting up DocumentSectionsExtension.");

        if (renderer is HtmlRenderer htmlRenderer)
        {
            // Remove any existing renderers for HeadingBlock
            var toRemove = htmlRenderer.ObjectRenderers
                .Where(r => r is HtmlObjectRenderer<HeadingBlock>)
                .ToList();

            foreach (var r in toRemove)
                htmlRenderer.ObjectRenderers.Remove(r);

            // Now insert yours at the top
            var sectionRenderer = new DocumentSectionRenderer();
            htmlRenderer.ObjectRenderers.Insert(0, sectionRenderer);


            htmlRenderer.ObjectRenderers.ReplaceOrAdd<FinalizingDocumentRenderer>(
                new FinalizingDocumentRenderer(sectionRenderer)
            );
        }
    }
}
