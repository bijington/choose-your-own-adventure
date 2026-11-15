using Microsoft.Maui.Graphics;
using Font = Microsoft.Maui.Graphics.Font;

namespace ChooseYourOwnAdventure;

/// <summary>
/// Fonts used by the code samples rendered through MAUI Graphics.
/// Cascadia Code is embedded as a MAUI font and registered with the
/// "CascadiaCode" alias in MauiProgram.
/// </summary>
public static class CodeFonts
{
#if ANDROID
    // MAUI Graphics currently resolves packaged fonts more reliably by filename on Android.
    private const string FontName = "CascadiaCode-VariableFont_wght.ttf";
#elif IOS || MACCATALYST
    // Apple's graphics canvas resolves the font by its bundled PostScript name.
    private const string FontName = "CascadiaCode-Regular";
#else
    private const string FontName = "CascadiaCode";
#endif

    public static Font Regular { get; } = new(FontName, 400);

    public static Font SemiBold { get; } = new(FontName, 600);

    public static Font Bold { get; } = new(FontName, 700);
}
