using Markdig;

namespace BlakePlugin.DocsRenderer.MarkdownExtensions;

public static class MarkdigExtensions
{
    public static MarkdownPipelineBuilder UsePrism(this MarkdownPipelineBuilder pipeline)
    {
        PrismOptions options = new();
        pipeline.Extensions.Add(new PrismExtension(options));
        return pipeline;
    }

    public static MarkdownPipelineBuilder UsePrism(this MarkdownPipelineBuilder pipeline, PrismOptions options)
    {
        options ??= new PrismOptions();
        pipeline.Extensions.Add(new PrismExtension(options));
        return pipeline;
    }

    public static MarkdownPipelineBuilder UseDocumentSections(this MarkdownPipelineBuilder pipeline)
    {
        pipeline.Extensions.Add(new DocumentSectionsExtension());
        return pipeline;
    }
}
