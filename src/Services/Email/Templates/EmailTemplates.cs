using System.Net;
using System.Text.RegularExpressions;

namespace Email.Templates;

public record RenderedEmail(string Subject, string TextBody, string? HtmlBody);

// Renders the templates embedded in this folder. Each email has a {Name}.txt, whose first line
// is "Subject: ...", and optionally a {Name}.html. {{Field}} placeholders are filled from the
// values given, HTML-encoded in the HTML version.
public partial class EmailTemplates
{
    public RenderedEmail Render(string name, IReadOnlyDictionary<string, string> values)
    {
        var text = Load($"{name}.txt") ?? throw new ArgumentException($"No email template named {name}", nameof(name));
        var html = Load($"{name}.html");

        var lines = text.ReplaceLineEndings("\n").Split('\n');
        if (!lines[0].StartsWith("Subject:"))
            throw new InvalidOperationException($"Email template {name}.txt must start with a Subject: line");

        var subject = Fill(lines[0]["Subject:".Length..].Trim(), values, encode: false);
        var body = Fill(string.Join('\n', lines.Skip(1)).Trim('\n'), values, encode: false);

        return new RenderedEmail(subject, body, html == null ? null : Fill(html, values, encode: true));
    }

    private static string Fill(string template, IReadOnlyDictionary<string, string> values, bool encode) =>
        Placeholder().Replace(template, match =>
        {
            var key = match.Groups[1].Value;
            if (!values.TryGetValue(key, out var value))
                throw new InvalidOperationException($"Email template has no value for {{{{{key}}}}}");
            return encode ? WebUtility.HtmlEncode(value) : value;
        });

    private static string? Load(string fileName)
    {
        var assembly = typeof(EmailTemplates).Assembly;
        using var stream = assembly.GetManifestResourceStream($"{typeof(EmailTemplates).Namespace}.{fileName}");
        if (stream == null) return null;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex Placeholder();
}
