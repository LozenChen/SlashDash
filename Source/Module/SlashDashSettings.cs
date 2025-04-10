using YamlDotNet.Serialization;

namespace Celeste.Mod.SlashDash.Module;

[SettingName("Slash Dash")]
public class SlashDashSettings : EverestModuleSettings {

    public static SlashDashSettings Instance { get; private set; }

    public SlashDashSettings() {
        Instance = this;
    }

    public void Initialize() {
        ExSlash.BaseGenerationRate = GenerationRate * 0.01f;
        ExSlash.MaxNextGenerationCount = MaxNextGenerationCount;
    }

    [YamlMember(Alias = "Enabled")]

    public bool enabled = true;

    [YamlIgnore]
    public bool Enabled {
        get => (SlashDashSession.Instance?.OverrideEnabled).GetValueOrDefault(enabled);
        set {
            enabled = value;
            if (SlashDashSession.Instance is { } instance) {
                instance.OverrideEnabled = null;
            }
        }
    }

    [SettingRange(1, 40)]
    public int MainSlashLength { get; set; } = 10;

    public int generationRate = 70;

    [SettingRange(0, 99)]
    public int GenerationRate { get => generationRate; set { generationRate = value; ExSlash.BaseGenerationRate = value * 0.01f; } }

    public int maxNextGenerationCount = 5;

    [SettingRange(0, 50)]
    public int MaxNextGenerationCount { get => maxNextGenerationCount; set { maxNextGenerationCount = value; ExSlash.MaxNextGenerationCount = value; } }
}