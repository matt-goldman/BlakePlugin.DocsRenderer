using Blake.BuildTools;
using BlakePlugin.DocsRenderer.MarkdownExtensions;

namespace BlakePlugin.DocsRenderer;

public class Plugin : IBlakePlugin
{
    private static readonly PrismOptions _options = new()
    {
        UseLineNumbers = true,
        UseCopyButton = true,
        UseLineHighlighting = true,
        UseLineDiff = true
    };

    public Task BeforeBakeAsync(BlakeContext context)
    {
        // This method is called before the bake process starts
        // You can add any pre-bake logic here if needed

        context.PipelineBuilder
            .UsePrism(_options)
            .UseDocumentSections();

        return Task.CompletedTask;
    }
}
