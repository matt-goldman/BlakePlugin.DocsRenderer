using Markdig;
using Microsoft.Extensions.Logging;

namespace BlakePlugin.DocsRenderer.MarkdownExtensions;

public static class MarkdigExtensions
{
    public static MarkdownPipelineBuilder UsePrism(this MarkdownPipelineBuilder pipeline, ILogger? logger = null)
    {
        PrismOptions options = new();
        pipeline.Extensions.Add(new PrismExtension(options, logger));
        return pipeline;
    }

    public static MarkdownPipelineBuilder UsePrism(this MarkdownPipelineBuilder pipeline, PrismOptions options, ILogger? logger = null)
    {
        options ??= new PrismOptions();
        pipeline.Extensions.Add(new PrismExtension(options, logger));
        return pipeline;
    }

    public static MarkdownPipelineBuilder UseDocumentSections(this MarkdownPipelineBuilder pipeline, ILogger? logger = null)
    {
        pipeline.Extensions.Add(new DocumentSectionsExtension(logger));
        return pipeline;
    }
}
