using Microsoft.Extensions.Configuration;

namespace ChooseYourOwnAdventure.Presentation;

public sealed class TalkSummaryOptions
{
    public string SpeakerName { get; set; } = "Shaun Lawrence";
    public string ResourcesUrl { get; set; } = string.Empty;

    public Uri? ResourcesUri => Uri.TryCreate(ResourcesUrl.Trim(), UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp) ? uri : null;

    public static TalkSummaryOptions FromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection("TalkSummary");
        return new TalkSummaryOptions
        {
            SpeakerName = string.IsNullOrWhiteSpace(section[nameof(SpeakerName)])
                ? "Shaun Lawrence" : section[nameof(SpeakerName)]!.Trim(),
            ResourcesUrl = section[nameof(ResourcesUrl)] ?? string.Empty
        };
    }
}
