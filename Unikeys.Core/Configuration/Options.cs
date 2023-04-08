using Newtonsoft.Json.Linq;

namespace Unikeys.Core.Configuration;

public static class Options
{
    static Options()
    {
        if (!File.Exists("appsettings.json"))
            throw new FileNotFoundException("Settings file has not been found", "appsettings.json");

        var json = JObject.Parse(File.ReadAllText("appsettings.json"));

        Theme = (ThemeType)(int)(json[nameof(Theme)] ?? 0);

        SFXPaths = json["SFXModules"]?.ToObject<SFXPaths>() ?? new SFXPaths();
    }

    private static void SaveOptions()
    {
        var json = JObject.Parse(File.ReadAllText("appsettings.json"));

        json[nameof(Theme)] = (int)Theme;

        File.WriteAllText("appsettings.json", json.ToString());
    }

    private static ThemeType _theme;
    public static ThemeType Theme
    {
        get => _theme;
        set
        {
            _theme = value;
            SaveOptions();
        }
    }

    public static SFXPaths SFXPaths { get; private set; }
}
