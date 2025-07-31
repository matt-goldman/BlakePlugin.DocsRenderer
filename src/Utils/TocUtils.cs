using Blake.Types;
using BlakePlugin.DocsRenderer.Types;

namespace BlakePlugin.DocsRenderer.Utils;

public static class TocUtils
{
    /// <summary>
    /// Generates a Table of Contents (TOC) for the site based on the provided pages.
    /// </summary>
    /// <param name="pages">The list of pages to include in the TOC.</param>
    /// <param name="slugSegmentsToSkip">The number of slug segments to exclude from the TOC hierarchy. Defaults to 0.</param>
    /// <returns>A <see cref="List{TocNode}"/> representing the site structure as TOC nodes.</returns>
    /// <remarks>Note that skipped slug segments are only excluded from the TOC hierarchy, they are not removed from the actual slugs.</remarks>
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

        // Sort pages by Slug by default
        foreach (var page in pages.OrderBy(p => p.Slug))
        {
            Console.WriteLine($"Processing page: {page.Slug} - {page.Title}");

            var slugParts = page.Slug.Trim('/').Split('/');

            var current = root;

            current.SortOrder = GetSortOrder(page);

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
                        Id          = segment,
                        Text        = isLeaf ? page.Title : segment,
                        Slug        = slugPath,
                        Children    = [],
                        SortOrder   = isLeaf ? GetSortOrder(page) : 0
                    };

                    current.Children.Add(child);
                }

                current = child;
            }

            current.Children.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
        }

        root.Children.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));

        return root.Children;
    }

    private static int GetSortOrder(PageModel page)
    {
        if (page.Metadata.TryGetValue("pageOrder", out string? value) && int.TryParse(value, out int order))
        {
            return order;
        }
        // Default sort order if not specified
        return 0;
    }
}
