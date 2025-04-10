using Celeste.Mod.Entities;
using Celeste.Mod.SlashDash.Module;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SlashDash.Triggers;

[CustomEntity("SlashDash/SlashDashTrigger")]
[Tracked]
public class SlashDashTrigger : Trigger {
    public bool OneUse;

    public bool Enable;

    public SlashDashTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        OneUse = data.Bool("OneUse", true);
        Enable = data.Bool("Enable", true);
    }

    public override void OnEnter(Player player) {
        if (SlashDashSession.Instance is { } instance){
            instance.OverrideEnabled = Enable;
        }
        base.OnEnter(player);
        if (OneUse) {
            RemoveSelf();
        }
        Logger.Log("SlashDash", $"{this.GetType().Name} triggered, set SlashDashSettings.Enabled = {Enable}.");
    }
}