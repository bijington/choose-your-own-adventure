using System.Text;
using System.Text.RegularExpressions;

namespace ChooseYourOwnAdventure.PresenterNotes;

/// <summary>Extracts speaker prose from the maintained talk guide, excluding code blocks.</summary>
public sealed class PresenterNotesGuide
{
    private readonly Dictionary<string, string> _notes = new(StringComparer.Ordinal);

    public PresenterNotesGuide(string markdown)
    {
        string? scene = null;
        var content = new StringBuilder();
        var inCode = false;
        var collect = true;
        void Save()
        {
            if (scene is not null) _notes[scene] = content.ToString().Trim();
            content.Clear();
        }
        foreach (var line in markdown.Replace("\r", "").Split('\n'))
        {
            if (line.StartsWith("```", StringComparison.Ordinal)) { inCode = !inCode; continue; }
            if (inCode) continue;
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                Save();
                var match = Regex.Match(line, "`([A-Za-z][A-Za-z0-9]*Scene)`");
                scene = match.Success ? match.Groups[1].Value : null;
                collect = true;
                continue;
            }
            if (line.StartsWith("# ", StringComparison.Ordinal)) { Save(); scene = null; }
            if (scene is null || line.Trim() == "---" || line.StartsWith('|')) continue;
            if (line.StartsWith("### ", StringComparison.Ordinal))
            {
                var heading = line[4..].ToLowerInvariant();
                collect = heading.Contains("talking") || heading.Contains("key message")
                    || heading.Contains("transition") || heading.Contains("purpose")
                    || heading.Contains("if running late") || heading.Contains("teaching");
                if (collect) content.AppendLine();
                continue;
            }
            if (!collect) continue;
            var plain = Regex.Replace(line.TrimStart('#', ' ', '>'), "[`*]", "");
            plain = Regex.Replace(plain, @"\[([^\]]+)\]\([^\)]+\)", "$1");
            content.AppendLine(plain);
        }
        Save();
    }

    public string ForScene(string sceneName) => _notes.GetValueOrDefault(sceneName,
        "Introduce this spread, then turn the page when the audience is ready.");
}
