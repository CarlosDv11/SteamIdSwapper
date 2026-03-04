using System.Text.Json;

namespace RE5SteamIdSwapper;

public class AppSettings
{
    public string SteamId    { get; set; } = "";
    public string RemotePath { get; set; } = "";
}

public static class SettingsManager
{
    private static readonly string SettingsDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                     "RE5SteamIdSwapper");

    private static readonly string SettingsFile =
        Path.Combine(SettingsDir, "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                string json = File.ReadAllText(SettingsFile);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch { /* retorna padrão em caso de arquivo corrompido */ }

        return new AppSettings();
    }

    public static void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(SettingsDir);
            string json = JsonSerializer.Serialize(settings,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFile, json);
        }
        catch { /* falha silenciosa, não crítico */ }
    }
}
