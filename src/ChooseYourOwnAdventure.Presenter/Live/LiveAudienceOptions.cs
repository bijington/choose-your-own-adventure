using Microsoft.Extensions.Configuration;

namespace ChooseYourOwnAdventure.Live;

public sealed class LiveAudienceOptions
{
    // Safe local defaults; appsettings.Development.json overrides these in Debug builds.
    public string BaseUrl { get; set; } = "http://localhost:5187";
    public string SessionId { get; set; } = "ORBIT";
    public string PresenterKey { get; set; } = "orbit-local-presenter";

    public static LiveAudienceOptions FromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection("LiveAudience");
        var defaults = new LiveAudienceOptions();
        return new LiveAudienceOptions
        {
            BaseUrl = section[nameof(BaseUrl)] ?? defaults.BaseUrl,
            SessionId = section[nameof(SessionId)] ?? defaults.SessionId,
            PresenterKey = section[nameof(PresenterKey)] ?? defaults.PresenterKey
        };
    }

    public string JoinUrl => $"{BaseUrl.TrimEnd('/')}/join/{SessionId}";
    public string HubUrl => $"{BaseUrl.TrimEnd('/')}/adventureHub";
}
