using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System.Reflection;

namespace Celeste.Mod.SlashDash.Module;

public class SlashDashModule : EverestModule {

    public static SlashDashModule Instance;

    public static SlashDashSettings Settings => SlashDashSettings.Instance;
    public SlashDashModule() {
        Instance = this;
    }

    public override Type SettingsType => typeof(SlashDashSettings);

    public override void Load() {
        Everest.Events.Level.OnLoadLevel += Level_OnLoadLevel;
    }

    private void Level_OnLoadLevel(Level level, Player.IntroTypes playerIntro, bool isFromLoader) {
        ExSlash.LivingSlashes = 0;
        ExSlash.DashCount = 0;
    }

    public override void Unload() {
        Everest.Events.Level.OnLoadLevel -= Level_OnLoadLevel;
        HookHelper.Unload();
    }

    public override void Initialize() {
        typeof(Player).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget().IlHook(IL_Player_DashCoroutine);
    }

    public override void LoadSettings() {
        base.LoadSettings();
        SlashDashSettings.Instance.Initialize();
    }

    private void IL_Player_DashCoroutine(ILContext il) {
        ILCursor cursor = new ILCursor(il);
        bool success = false;
        if (cursor.TryGotoNext(ins => ins.MatchCall<SlashFx>(nameof(SlashFx.Burst)), ins => ins.OpCode == OpCodes.Pop)) {
            cursor.Index++;
            cursor.EmitDelegate(VanillaDashSlashModifier);
            cursor.Index++;
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.EmitDelegate(AddMoreSlash);
            success = true;
        }

        if (success) {
            Logger.Log(LogLevel.Verbose, "SlashDash", "Hook Succeeds");
        }
        else {
            Logger.Log(LogLevel.Warn, "SlashDash", "Hook Fails");
        }
    }

    private static SlashFx VanillaDashSlashModifier(SlashFx slashFx) {
        if (!SlashDashModule.Settings.Enabled) {
            return slashFx;
        }
        slashFx.Sprite.Scale.X *= Rand.Rnd.Range(0.6f, 1.4f) * SlashDashModule.Settings.MainSlashLength;
        float rand = Rand.Rnd.Range(-0.2f, 0.2f);
        float dir = slashFx.Direction.Angle() + rand;
        slashFx.Direction = Calc.AngleToVector(dir, 1f);
        if (Math.Abs(dir - MathF.PI) > 0.01f) {
            slashFx.Sprite.Rotation = dir;
        }
        Color color = Rand.GetColor();
        slashFx.Sprite.Color = Color.Lerp(Color.White, color, 0.3f);
        return slashFx;
    }

    private static void AddMoreSlash(Player player) {
        if (!SlashDashModule.Settings.Enabled) {
            return;
        }
        ExSlash.DashCount++;
        // ExSlash.Burst(player.Position + player.Speed * Engine.DeltaTime * Rand.Rnd.Range(10f, 20f) + player.Speed.Rotate(MathF.PI/2f) * Engine.DeltaTime * Rand.Rnd.Range(-2f, 2f), player.DashDir.Angle() + MathF.PI / 2f + Rand.Rnd.Range(-0.5f, 0.5f), Rand.GetColor(400), Rand.Rnd.Range(0.1f, 0.3f) * SlashDashModule.Settings.MainSlashLength, Rand.Rnd.Range(0.05f, 0.5f), Rand.Rnd.Range(6f, 30f));
        ExSlash.ExplosiveRandBurst(player.Position, player.Speed.Length());
    }
}




