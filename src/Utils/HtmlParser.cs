using BlakePlugin.DocsRenderer.Types;
using HtmlAgilityPack;
using System.Text;

namespace BlakePlugin.DocsRenderer.Utils;

internal static class HtmlParser
{
    internal static string AddDocumentSections(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var body = doc.DocumentNode;
        var allNodes = body.ChildNodes.ToList();
        var newHtml = new StringBuilder();
        List<Section> rootDocumentSections = [];
        Stack<(Section DocumentSection, int level)> documentSectionStack = new Stack<(Section, int)>();

        newHtml.Append("<div>"); // Root container (optional)

        foreach (var node in allNodes)
        {
            if (node.Name is "h1" or "h2" or "h3" or "h4" or "h5" or "h6")
            {
                var level = int.Parse(node.Name.Substring(1));
                var headingId = node.GetAttributeValue("id", "");
                node.Attributes.Remove("id");

                // Create the new DocumentSection object
                var newDocumentSection = new Section
                {
                    Id = headingId,
                    Text = node.InnerText
                };

                // Close DocumentSections if moving up levels
                while (documentSectionStack.Count > 0 && documentSectionStack.Peek().level >= level)
                {
                    documentSectionStack.Pop();
                    newHtml.Append("</section>");
                }

                // Add to parent DocumentSection or root
                if (documentSectionStack.Count > 0)
                {
                    documentSectionStack.Peek().DocumentSection.Children.Add(newDocumentSection);
                }
                else
                {
                    rootDocumentSections.Add(newDocumentSection);
                }

                documentSectionStack.Push((newDocumentSection, level));

                // Open new DocumentSection in HTML
                newHtml.Append($"<Section{(string.IsNullOrEmpty(headingId) ? "" : $" id=\"{headingId}\"")}>");
            }

            // Append the actual content
            newHtml.Append(node.OuterHtml);
        }

        // Close any remaining DocumentSections
        while (documentSectionStack.Count > 0)
        {
            newHtml.Append("</Section>");
            documentSectionStack.Pop();
        }

        newHtml.Append("</div>"); // Close root container

        var jsonSections = System.Text.Json.JsonSerializer.Serialize(rootDocumentSections);

        newHtml.Append($"<!-- blake:sections:{jsonSections} -->");

        return newHtml.ToString();
    }
}
