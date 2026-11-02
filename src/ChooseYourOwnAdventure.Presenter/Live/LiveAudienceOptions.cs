namespace ChooseYourOwnAdventure.Live;

public sealed class LiveAudienceOptions
{
    // Local development default. Replace with your Azure App Service URL for the talk.
    public string BaseUrl { get; set; } = "http://localhost:5187";
    public string SessionId { get; set; } = "ORBIT";

    public string JoinUrl => $"{BaseUrl.TrimEnd('/')}/join/{SessionId}";
    public string HubUrl => $"{BaseUrl.TrimEnd('/')}/adventureHub";
}
