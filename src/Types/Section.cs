namespace BlakePlugin.DocsRenderer.Types;

public class Section
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public int SortOrder { get; set; } = 0;
    public List<Section> Children { get; set; } = [];
}
