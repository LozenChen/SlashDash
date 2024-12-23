namespace Celeste.Mod.SlashDash.Module;

[SettingName("Slash Dash")]
public class SlashDashSettings : EverestModuleSettings {

    public static SlashDashSettings Instance { get; private set; }

    public SlashDashSettings() {
        Instance = this;
    }

    public bool Enabled { get; set; } = true;

    [SettingRange(1, 40)]
    public int MainSlashLength { get; set; } = 10;

    public int generationRate = 30;

    [SettingRange(1, 90)]
    public int GenerationRate { get => generationRate; set { generationRate = value; f_GenerationRate = value * 0.01f; } }


    [SettingRange(1, 5)]
    public int MaxNextGenerationCount { get; set; } = 3;

    public float f_GenerationRate = 0.6f;
}
