namespace BlakePlugin.DocsRenderer.Types;

public class TocNode : Section
{
    public required string Slug { get; set; }

    public new List<TocNode> Children { get; set; } = new();
}
