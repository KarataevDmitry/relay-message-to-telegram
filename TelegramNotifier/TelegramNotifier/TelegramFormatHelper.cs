using System.Text.RegularExpressions;
using Markdig;

namespace TelegramNotifier;

/// <summary>
/// Converts Markdown to Telegram-allowed HTML so that WTelegramClient's HtmlToEntities can be used.
/// Telegram supports only: &lt;b&gt;, &lt;i&gt;, &lt;code&gt;, &lt;pre&gt;, &lt;a href=""&gt;, &lt;s&gt;, &lt;u&gt;, &lt;tg-spoiler&gt;.
/// </summary>
public static class TelegramFormatHelper
{
    /// <summary>
    /// Converts Markdown to Telegram-compatible HTML (flat tags, no nesting).
    /// Then use Client.HtmlToEntities(ref html) and SendMessageAsync(peer, html, entities).
    /// </summary>
    public static string MdToTelegramHtml(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return markdown;

        string html = Markdown.ToHtml(markdown.Trim());

        // Telegram uses <b>/<i>, not <strong>/<em>
        html = html.Replace("<strong>", "<b>").Replace("</strong>", "</b>");
        html = html.Replace("<em>", "<i>").Replace("</em>", "</i>");

        // Headings → bold + newline (Telegram has no heading entity)
        html = Regex.Replace(html, @"<h[1-6]\b[^>]*>(.*?)</h[1-6]>", "<b>$1</b>\n", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // Paragraphs: strip tag, keep newlines
        html = Regex.Replace(html, @"</p>\s*<p>", "\n\n", RegexOptions.Singleline);
        html = Regex.Replace(html, @"<p\b[^>]*>|</p>", "\n", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // Lists: Telegram has no list entity; leave <ul>/<li> as-is and strip to plain + newlines, or keep • in text. Strip list tags for clean entity-less text.
        html = Regex.Replace(html, @"<ul\b[^>]*>|</ul>", "\n", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        html = Regex.Replace(html, @"<ol\b[^>]*>|</ol>", "\n", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        html = Regex.Replace(html, @"<li\b[^>]*>(.*?)</li>", "• $1\n", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // Blockquote: strip or keep as text; strip tags
        html = Regex.Replace(html, @"<blockquote\b[^>]*>|</blockquote>", "\n", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // <pre><code>...</code></pre> → <pre>...</pre> (Telegram accepts pre)
        html = Regex.Replace(html, @"<pre\b[^>]*>\s*<code[^>]*>(.*?)</code>\s*</pre>", "<pre>$1</pre>", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        return html.Trim();
    }
}
