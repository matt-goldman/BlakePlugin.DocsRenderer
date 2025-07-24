using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using System.Diagnostics;

namespace BlakePlugin.DocsRenderer.MarkdownExtensions;

public class FinalizingDocumentRenderer(DocumentSectionRenderer sectionRenderer) : HtmlObjectRenderer<MarkdownDocument>
{
    protected override void Write(HtmlRenderer renderer, MarkdownDocument document)
    {
        Console.WriteLine("[BlakePlugin.DocsRenderer] Finalizing document rendering.");

        foreach (var block in document)
        {
            Console.WriteLine($"[BlakePlugin.DocsRenderer] Processing block: {block.GetType().Name}");

            renderer.Write(block); // delegate to existing block renderers
        }

        sectionRenderer.CloseRemaining(renderer);
    }
}
