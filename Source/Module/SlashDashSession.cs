
namespace Celeste.Mod.SlashDash.Module;

public class SlashDashSession : EverestModuleSession {

    public static SlashDashSession Instance => (SlashDashSession)SlashDashModule.Instance._Session;

    public bool? OverrideEnabled;
}