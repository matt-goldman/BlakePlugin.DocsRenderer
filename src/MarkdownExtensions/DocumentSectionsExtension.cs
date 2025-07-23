using Markdig;
using Markdig.Renderers;

namespace BlakePlugin.DocsRenderer.MarkdownExtensions;

public class DocumentSectionsExtension : IMarkdownExtension
{
    public DocumentSectionRenderer? Renderer { get; private set; }

    public void Setup(MarkdownPipelineBuilder pipeline) { }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is HtmlRenderer htmlRenderer)
        {
            Renderer = new DocumentSectionRenderer();
            htmlRenderer.ObjectRenderers.ReplaceOrAdd<DocumentSectionRenderer>(Renderer);

            htmlRenderer.ObjectRenderers.ReplaceOrAdd<FinalizingDocumentRenderer>(
                new FinalizingDocumentRenderer(Renderer)
            );
        }
    }
}
