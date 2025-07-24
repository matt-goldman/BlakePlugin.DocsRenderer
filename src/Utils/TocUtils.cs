using Blake.Types;
using BlakePlugin.DocsRenderer.Types;
using System.Diagnostics;

namespace BlakePlugin.DocsRenderer.Utils;

public static class TocUtils
{
    public static List<TocNode> BuildSiteTocNodes(List<PageModel> pages)
    {
        var root = new TocNode
        {
            Id = "root",
            Text = "Root",
            Slug = "/",
            Children = []
        };

        Console.WriteLine($"Building TOC with {pages.Count} pages.");

        foreach (var page in pages.OrderBy(p => p.Slug))
        {
            Console.WriteLine($"Processing page: {page.Slug} - {page.Title}");

            var slugParts = page.Slug.Trim('/').Split('/');
            var current = root;

            for (int i = 0; i < slugParts.Length; i++)
            {
                // Skip empty segments (in case of leading/trailing slashes)
                var segment = slugParts[i];
                var isLeaf = i == slugParts.Length - 1;
                var slugPath = "/" + string.Join('/', slugParts.Take(i + 1));

                var child = current.Children.FirstOrDefault(c => c.Id == segment);

                if (child == null)
                {
                    Console.WriteLine($"Creating new TOC node: {segment} (isLeaf: {isLeaf})");

                    child = new TocNode
                    {
                        Id = segment,
                        Text = isLeaf ? page.Title : segment,
                        Slug = slugPath,
                        Children = []
                    };
                    current.Children.Add(child);
                }

                current = child;
            }
        }

        return root.Children;
    }
}
