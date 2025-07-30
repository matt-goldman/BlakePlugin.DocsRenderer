using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace BlakePlugin.DocsRenderer.MarkdownExtensions;

public class PrismExtension(PrismOptions options) : IMarkdownExtension
{

    public void Setup(MarkdownPipelineBuilder pipeline)
    {
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);

        if (renderer is HtmlRenderer htmlRenderer)
        {
            var codeBlockRenderer = htmlRenderer.ObjectRenderers.FindExact<CodeBlockRenderer>();

            if (codeBlockRenderer != null)
            {
                htmlRenderer.ObjectRenderers.Remove(codeBlockRenderer);
            }

            htmlRenderer.ObjectRenderers.AddIfNotAlready(new PrismCodeBlockRenderer(codeBlockRenderer!, options));
        }
    }
}
