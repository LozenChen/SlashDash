using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SlashDash.Module;

[Tracked(false)]
[Pooled]
public class ExSlash : Entity {

    public Sprite Sprite;

    public Vector2 Direction;

    public float Delay;

    public float MovingSpeed;

    public float NextGenerationPossibility;

    public int Generation;

    public static int LivingSlashes = 0;

    public static int MaxCapacity = 2000;
    public ExSlash() {
        LivingSlashes++;
        Add(Sprite = new Sprite(GFX.Game, "effects/slash/"));
        Sprite.Add("play", "", 0.1f, 0, 1, 2, 3);
        Sprite.CenterOrigin();
        Sprite.OnFinish = delegate {
            RemoveSelf();
        };
        base.Depth = -100;
    }

    public override void Removed(Scene scene) {
        base.Removed(scene);
        LivingSlashes--;
    }

    public override void Update() {
        if (Delay > 0f) {
            Delay -= Engine.DeltaTime;
            if (Delay <= 0f) {
                Sprite.Play("play", restart: true);
                Visible = true;
            }
        }
        else {
            Position += Direction * MovingSpeed * Engine.DeltaTime;
            base.Update();
        }
    }

    public static ExSlash Burst(Vector2 position, float direction, Color color, float length = 1f, float delay = 0f, float movingSpeed = 8f) {
        Scene scene = Engine.Scene;
        ExSlash slashFx = Engine.Pooler.Create<ExSlash>();
        scene.Add(slashFx);
        slashFx.Position = position;
        slashFx.Direction = Calc.AngleToVector(direction, length);
        if (delay <= 0f) {
            slashFx.Sprite.Play("play", restart: true);
            slashFx.Visible = true;
        }
        else {
            slashFx.Delay = delay;
            slashFx.Visible = false;
        }
        slashFx.Sprite.Scale = new Vector2(length, 1f);
        slashFx.Sprite.Rotation = 0f;
        slashFx.MovingSpeed = movingSpeed;
        if (Math.Abs(direction - MathF.PI) > 0.01f) {
            slashFx.Sprite.Rotation = direction;
        }
        slashFx.Active = true;
        slashFx.Sprite.Color = color;

        return slashFx;
    }

    public static ExSlash ExplosiveRandBurst(Vector2 position, float offset) {
        ExSlash slash = RandBurst(position, offset);
        slash.NextGenerationPossibility = SlashDashModule.Settings.f_GenerationRate;
        SetGeneration(slash);
        return slash;
    }

    public static ExSlash RandBurst(Vector2 position, float offset) {
        return Burst(position + Rand.Rnd.Range(-Vector2.One / 10f, Vector2.One / 10f) * offset, Rand.Rnd.Range(-MathF.PI, MathF.PI), Rand.GetColor(), Rand.Rnd.Range(0.5f, 6f), Rand.Rnd.Range(0.05f, 0.7f), Rand.Rnd.Range(3f, 120f));
    }

    public static void SetGeneration(ExSlash slash) {
        if (LivingSlashes > MaxCapacity) {
            return;
        }
        slash.Sprite.OnFinish += (_) => {
            if (Rand.Rnd.Chance(slash.NextGenerationPossibility)) {
                if (slash.Generation <= 10) {
                    int tryCount = SlashDashModule.Settings.MaxNextGenerationCount;
                    for (int i = 1; i <= tryCount; i++) {
                        ExSlash nextGeneration = RandBurst(slash.Position, slash.MovingSpeed);
                        nextGeneration.NextGenerationPossibility = slash.NextGenerationPossibility * SlashDashModule.Settings.f_GenerationRate;
                        nextGeneration.Generation = slash.Generation + 1;
                        SetGeneration(nextGeneration);
                    }
                }
                else {
                    // wow
                }
            }
        };
    }
}

internal static class Rand {
    public static Random Rnd = new Random();

    public static void SetSeed(int seed) {
        Rnd = new Random(seed);
    }

    public static Color GetColor(int whiteness = 255) {
        int r = Rnd.Range(0, 255);
        int g = Rnd.Range(0, 255);
        int b = Rnd.Range(0, 255);
        if (r + g + b < whiteness) {
            r = 255 - r;
            g = 255 - g;
            b = 255 - b;
        }
        return new Color(r, g, b);
    }
}