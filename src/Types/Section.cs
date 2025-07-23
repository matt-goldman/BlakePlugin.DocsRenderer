namespace BlakePlugin.DocsRenderer.Types;

public class Section
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public List<Section> Children { get; set; } = [];
}
