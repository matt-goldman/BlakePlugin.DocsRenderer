# BlakePlugin.DocsRenderer

A plugin for [Blake](https://github.com/matt-goldman/blake) that adds scrollspy, TOC navigation, and beautiful code blocks to your documentation.

The plugin essentially comprises of three main parts:

* **Document Sectioning**: Automatically splits documentation pages into sections based on headings, generating a table of contents (TOC) and enabling in-page navigation.
* **Custom Code Rendering**: Enhances code blocks with custom Prism.js rendering, including line
numbering, line diffing, and other features.
* **Navigation Components**: Provides Razor components for site-wide and in-page table of contents, making it easy to integrate into your Blazor applications.

These are designed to work together, but you could theoretically use them separately if you only need one or two of the features. See below for more details on how to use each part.

## Features

* In-page navigation with scrollspy
* Document sectioning
* Custom Prism code rendering
* Customizable styles
* Includes default Bootstrap styles (works with the default Blazor template)
* Default Razor components for navigation and sectioning

## How it works

The plugin adds a Markdig renderer to the Markdown pipeline to process documentation pages more intelligently (there's nothing stopping you adding this to your blog though, too!). It uses headings to automatically split the content into sections and generates a table of contents. The scrollspy functionality highlights the current section in the navigation as the user scrolls through the page.

The plugin adds a custom Prism code renderer to the pipeline that enhances the default behavior by adding custom line numbering, line diffing, and other features. The included styles and JavaScript add visual and functional enhancements to code blocks.

## Installation

1. Add the package:

    ```bash
    dotnet add package BlakePlugin.DocsRenderer
    ```

2. Import the necessary styles and scripts in your `index.html`, and initialise the DocsRenderer:

    ```html
    <!-- Add this in the <head> section of your index.html file.
        Includes styles for the included site and page TOC components -->
    <link rel="stylesheet" href="_content/BlakePlugin.DocsRenderer/lib/css/plugin.css" />

    <!-- Add these before the closing </body> tag -->

    <!-- Configured version of Prism.js for code highlighting -->
    <script src="_content/BlakePlugin.DocsRenderer/lib/js/prism.js"></script> 
    <!-- DocsRenderer plugin script includes scrollspy logic and Prism extensions -->
    <script src="_content/BlakePlugin.DocsRenderer/lib/js/plugin.js"></script>
    ```

You will need to initialise the DocsRenderer plugin in order to use the components. The best place to do this is in your Blake template (i.e. `template.razor`):

```razor
@inject IJSRuntime js

// ...

@code {
    protected override async Task OnAfterRenderAsync(bool isFirstRender)
    {
        await base.OnAfterRenderAsync(isFirstRender);

        if (isFirstRender)
        {
            await js.InvokeVoidAsync("initializeDocsPlugin");
        }
    }
}
```

This ensures that the plugin is ready to use when your pages are rendered. The `initializeDocsPlugin` function is defined in the `plugin.js` file included with the package.

## Usage

### Razor Setup

Once the plugin is installed, you can use the built-in Razor components to render both the site-wide and in-page table of contents.

Make sure to import the relevant namespaces at the top of your `.razor` file:

```razor
@using BlakePlugin.DocsRenderer // Required for Section, note this is automatically added to generated pages
@using BlakePlugin.DocsRenderer.Components // Required for SiteToc and PageToc components
@using BlakePlugin.DocsRenderer.Utils // Required for TocNode and TocUtils
```

You also need to use `BlakePlugin.DocsRenderer.Types`, but if you're using these in your `template.razor` file, this is automatically added for you.

## Content Setup

You don't need to do anything special to your Markdown files; the plugin will automatically process them. However, you should ensure that your documentation pages are structured with headings (e.g., `#`, `##`, `###`) to enable sectioning and TOC generation.

Additionally, you can specify `pageOrder: <order>` in your front matter to control the order of pages in the TOC. But you don't have to do this; the plugin will automatically order pages based on their filenames if `pageOrder` is not specified.

> **Note**: It actually sorts using the default directory sort order. This is usually alphabetically by filename, but it can be affected by the filesystem or other factors. If you need a specific order, use the `pageOrder` front matter field.

### Adding navigation components

You can now use the `SiteToc` and `PageToc` components in your Razor pages or components, or `template.razor` files:

```razor
<SiteToc Pages="@contentIndex" />

<!-- Add your main page content here, or layout however you want -->

<PageToc Sections="@_sections" />
```

### Generating TOC and Section Data

In your `@code` block, you’ll typically initialise the site-wide table of contents like this:

```razor
@code {
    private List<TocNode> contentIndex = TocUtils.BuildSiteTocNodes(GeneratedContentIndex.GetPages());

    // This list of sections is automatically generated by the plugin
    private List<Section> _sections = [

        new Section { Text = "Introduction", Id = "introduction", Children = [ // ...] },
        new Section { Text = "Getting Started", Id = "getting-started", Children = [] /*Empty in this case*/ },
        // ...
    ];
}
```

The `_sections` list is automatically injected by the plugin into the generated Razor page at build time, so you don't need to maintain it manually unless you’re testing or debugging. (Note the comments were added just for this example, they are not present in the actual code.)

## Customisation

While the behaviour of the plugin is mostly fixed, you can use the generated data to drive your own components. The included components are trivial; you can copy the code and modify it to suit your needs (e.g. add your own styles or additional functionality).

While the components depend on Bootstrap, they do use specific namespaces CSS classes, so you can override the styles in your own CSS files if you want to change the appearance without affecting the functionality. Be careful of simply excluding the CSS include as it also has styling for code blocks and other elements that the plugin uses. You can do whatever works for you.

